#!/bin/bash
set -eo pipefail

# Enable error tracing
trap 'echo "Error at line $LINENO; exit code $?"' ERR

# Verify required command-line tools
required_cmds=(gh jq yq)
for cmd in "${required_cmds[@]}"; do
  command -v "$cmd" >/dev/null || { 
    echo "ERROR: Missing $cmd. Install with: brew install $cmd"
    exit 1
  }
done

# Configuration parameter handling
REPO_OWNER="${1:-${GITHUB_REPOSITORY_OWNER:-perkify-net}}"
REPO_NAME="${2:-${GITHUB_REPOSITORY#*/:-perkify}}"
MILESTONES_YAML_FILE="${3:-${MILESTONES_YAML_FILE:-.github/milestones.yml}}"
DRY_RUN="${4:-${DRY_RUN:-false}}"
[[ "$DRY_RUN" == "true" ]] && DRY_RUN=true || DRY_RUN=false

# Validate configuration file existence
validate_config() {
  [ -f "$MILESTONES_YAML_FILE" ] || {
    echo "ERROR: Config file $MILESTONES_YAML_FILE not found at: $(pwd)"
    exit 1
  }
}
validate_config

# GitHub API request handler
call_github_api() {
  local method=$1 endpoint=$2 data=$3
  local dry_run_msg="[DRY-RUN] Would execute: gh api -X $method $endpoint"
  
  if $DRY_RUN; then
    echo "$dry_run_msg with data: $data"
    return 0
  fi
  
  # Transmit data via stdin for better compatibility
  echo "$data" | gh api \
    --method "$method" \
    "$endpoint" \
    --input -
}

# Main synchronization workflow
echo "Syncing milestones with configuration:"
echo "  - Repository: $REPO_OWNER/$REPO_NAME"
echo "  - Config file: $(realpath "$MILESTONES_YAML_FILE")"
echo "  - Dry run: $DRY_RUN"

# Retrieve existing milestones with pagination support
fetch_existing_milestones() {
  declare -gA EXISTING_MILESTONES
  local api_response

  # Handle pagination (max 100 items per request)
  api_response=$(gh api "/repos/$REPO_OWNER/$REPO_NAME/milestones?state=all&per_page=100" --jq '.[] | @base64' || true)

  [ -z "$api_response" ] && return
  
  while IFS= read -r b64; do
    # Validate base64 decoding
    decoded=$(base64 -d <<< "$b64" 2>/dev/null) || {
      echo "Warning: Failed to decode milestone data: $b64"
      continue
    }
    
    title=$(jq -r '.title // empty' <<< "$decoded")
    [ -n "$title" ] || continue
    
    EXISTING_MILESTONES["$title"]="$decoded"
  done <<< "$api_response"
}
fetch_existing_milestones

# Process target milestones from YAML configuration
parse_target_milestones() {
  declare -gA TARGET_MILESTONES
  local yaml_data

  # Validate YAML syntax
  yaml_data=$(yq eval -o json '.' "$MILESTONES_YAML_FILE") || {
    echo "ERROR: Invalid YAML structure in $MILESTONES_YAML_FILE"
    exit 1
  }

  while IFS= read -r b64; do
    decoded=$(base64 -d <<< "$b64" 2>/dev/null) || {
      echo "Warning: Invalid target milestone data: $b64"
      continue
    }
    
    title=$(jq -r '.title // empty' <<< "$decoded")
    [ -n "$title" ] || continue
    
    TARGET_MILESTONES["$title"]="$decoded"
  done < <(jq -cr '.milestones[] | @base64' <<< "$yaml_data")
}
parse_target_milestones

# Synchronization engine core logic
execute_sync() {
  for title in "${!TARGET_MILESTONES[@]}"; do
    local target_data=${TARGET_MILESTONES[$title]}
    
    # Data extraction with validation
    description=$(jq -r '.description // ""' <<< "$target_data")
    due_on=$(jq -r '.due_on? | select(. != null and . != "")' <<< "$target_data")
    state=$(jq -r '.state // "open"' <<< "$target_data")

    # Generate API payload with ISO 8601 date handling
    local request_data=$(jq -n \
      --arg desc "$description" \
      --arg due "${due_on:-null}" \
      --arg state "$state" \
      '{description: $desc, due_on: ($due | if . == "null" then null else . end), state: $state}')

    if [[ -n "${EXISTING_MILESTONES[$title]}" ]]; then
      local existing=${EXISTING_MILESTONES[$title]}
      local number=$(jq -r .number <<< "$existing")
      local current_data=$(jq -c '{description, due_on, state}' <<< "$existing")

      # Debugging output for data comparison
      echo "Comparing milestone: $title"
      echo "Existing configuration: $current_data"
      echo "Desired state: $request_data"

      if ! diff <(jq -S . <<< "$request_data") <(jq -S . <<< "$current_data") &>/dev/null; then
        echo "🔄 Updating existing milestone: \"$title\""
        call_github_api PATCH "/repos/$REPO_OWNER/$REPO_NAME/milestones/$number" "$request_data"
      fi
    else
      echo "🆕 Creating new milestone: \"$title\""
      call_github_api POST "/repos/$REPO_OWNER/$REPO_NAME/milestones" \
        "$(jq --arg title "$title" '. + {title: $title}' <<< "$request_data")"
    fi
  done
}
execute_sync

# Cleanup obsolete milestones
purge_obsolete_milestones() {
  for title in "${!EXISTING_MILESTONES[@]}"; do
    [ -n "${TARGET_MILESTONES[$title]}" ] && continue
    
    local existing=${EXISTING_MILESTONES[$title]}
    local number=$(jq -r .number <<< "$existing")
    local state=$(jq -r .state <<< "$existing")

    if [[ "$state" == "open" ]]; then
      echo "🚫 Closing obsolete milestone: \"$title\""
      call_github_api PATCH "/repos/$REPO_OWNER/$REPO_NAME/milestones/$number" \
        '{"state":"closed"}'
    fi
  done
}
purge_obsolete_milestones

echo "✅ Synchronization completed successfully"