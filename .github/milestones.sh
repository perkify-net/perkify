#!/bin/bash
set -eo pipefail
trap 'error_handler $LINENO' ERR
error_handler()
{
  local line=$1
  local exit_code=${2:-$?}
  echo "Error at line $line; exit code $exit_code" >&2
  exit "$exit_code"
}

# Verify required command-line tools
check_dependencies()
{
  local required_cmds=("$@")
  for cmd in "${required_cmds[@]}"; do
    command -v "$cmd" >/dev/null ||
    {
      echo "ERROR: Missing $cmd." >&2
      return 1
    }
  done
}
check_dependencies gh jq yq || exit 1

# Check existence of milestones configuration file
[ -f "$MILESTONES_YAML_FILE" ] ||
{
  echo "ERROR: Config file $MILESTONES_YAML_FILE not found."
  exit 1
}

# Detect dry-run mode and verbose mode
to_boolean()
{
  local default="${2:-false}"
  local input="${1:default}"
  local normalized=$(echo -n "${input}" | xargs | tr '[:upper:]' '[:lower:]')
  case "$normalized" in
    true|t|1|yes|y|on)   echo "true" ;;
    false|f|0|no|n|off)  echo "false" ;;
    *)                   echo "false" ;;
  esac
}
DRY_RUN=$(to_boolean "${DRY_RUN}" false)
VERBOSE=$(to_boolean "${VERBOSE}" false)
if [ "$DRY_RUN" = "true" ]; then
  echo "Dry run mode enabled."
fi
if [ "$VERBOSE" = "true" ]; then
  echo "Verbose mode enabled."
fi

# Utility to call GitHub API with dry-run & verbose support
call_github_api()
{
  local method=$1
  local endpoint=$2
  local data=$3

  # Validate JSON data before transmitting
  if ! jq empty <<< "$data" &>/dev/null; then
    echo "ERROR: Invalid JSON data" >&2
    return 1
  fi  
  
  # Simulate API request in dry-run mode  
  if [ "$DRY_RUN" = "true" ]; then
    echo "[DRY-RUN] gh api -X $method $endpoint"
    echo "[DRY-RUN] $data"
    return 0
  fi

  # Transmit data via stdin for better compatibility
  echo "$data" | gh api \
    --method "$method" \
    "$endpoint" \
    --input -
}

# Main synchronization workflow
echo "Syncing milestones with configuration..."
echo "Repository: $GITHUB_REPOSITORY"
echo "Config file: $(realpath "$MILESTONES_YAML_FILE")"
echo "Dry-run: $DRY_RUN"
echo "Verbose: $VERBOSE"

# Retrieve current milestones with pagination support
fetch_github_milestones()
{
  # Fetch all milestones with pagination support
  local response
  if ! response=$(gh api "/repos/$GITHUB_REPOSITORY/milestones?state=all&per_page=100" 2>&1); then
    echo "Error: Failed to fetch milestones. $response" >&2
    exit 1
  fi

  # Parse and store in glabal associative array for easy access
  declare -gA CURRENT_MILESTONES
  while IFS= read -r item; do
    title=$(jq -r '.title // empty' <<< "$item")
    [ -n "$title" ] && CURRENT_MILESTONES["$title"]="$item"
  done < <(jq -c '.[]' <<< "$response")
}
fetch_github_milestones
echo "Current Milestones: Total=${#CURRENT_MILESTONES[@]}"
if [ "$VERBOSE" = "true" ]; then
  for title in "${!CURRENT_MILESTONES[@]}"; do
    echo " - $title"
  done
fi

# Load target milestones from YAML configuration
load_target_milestones()
{
  # Validate YAML syntax
  local yaml_data=$(yq eval -o json '.' "$MILESTONES_YAML_FILE") ||
  {
    echo "ERROR: Invalid YAML structure in $MILESTONES_YAML_FILE"
    exit 1
  }

  # Parse and store in glabal associative array for easy access
  declare -gA TARGET_MILESTONES
  while IFS= read -r b64; do
    decoded=$(base64 -d <<< "$b64" 2>/dev/null) ||
    {
      echo "Warning: Invalid target milestone data: $b64"
      continue
    }
    
    title=$(jq -r '.title // empty' <<< "$decoded")
    [ -n "$title" ] || continue
    
    TARGET_MILESTONES["$title"]="$decoded"
  done < <(jq -cr '.milestones[] | @base64' <<< "$yaml_data")
}
load_target_milestones
echo "Target Milestones: Total=${#TARGET_MILESTONES[@]}"
if [ "$VERBOSE" = "true" ]; then
  for title in "${!TARGET_MILESTONES[@]}"; do
    echo " - $title"
  done
fi

# Synchronize
execute_sync()
{
  for title in "${!TARGET_MILESTONES[@]}"; do
    local target_data=${TARGET_MILESTONES[$title]}
    
    # Data extraction with validation
    description=$(jq -r '.description // ""' <<< "$target_data")
    due_on=$(jq -r '.due_on? | select(. != null and . != "")' <<< "$target_data")
    state=$(jq -r '.state // "open"' <<< "$target_data")

    # Convert to UTC time if due_on is provided
    if [ -n "$due_on" ]; then
      local utc_due_on=$(date -u -d "$due_on" '+%Y-%m-%dT%H:%M:%SZ' 2>/dev/null)
      if [ $? -ne 0 ]; then
        echo "ERROR: Invalid due_on format for '$title': $due_on" >&2
        continue
      fi
      due_on=$utc_due_on
    fi

    # Generate API payload with ISO 8601 date handling
    local request_data=$(jq -n \
      --arg desc "$description" \
      --arg due "${due_on:-null}" \
      --arg state "$state" \
      '{description: $desc, due_on: ($due | if . == "null" then null else . end), state: $state}')

    # Check if milestone already exists
    if [[ -n "${CURRENT_MILESTONES[$title]}" ]]; then
      local existing=${CURRENT_MILESTONES[$title]}
      local number=$(jq -r .number <<< "$existing")
      local current_data=$(jq -c '{description, due_on, state}' <<< "$existing")

      # Debugging output for data comparison
      echo "Comparing milestone: $title"
      echo "Current state: $current_data"
      echo "Desired state: $request_data"

      # Update only if there are changes or create new milestone when missing
      if ! diff <(jq -S . <<< "$request_data") <(jq -S . <<< "$current_data") &>/dev/null; then
        echo "🔄 Updating existing milestone: \"$title\""
        call_github_api PATCH "/repos/$GITHUB_REPOSITORY/milestones/$number" "$request_data"
      fi
    fi
    else
      # Create new milestone when missing
      echo "🆕 Creating new milestone: \"$title\""
      call_github_api POST "/repos/$GITHUB_REPOSITORY/milestones" \
        "$(jq --arg title "$title" '. + {title: $title}' <<< "$request_data")"
    fi
  done
}
execute_sync

# Cleanup obsolete milestones
purge_obsolete_milestones()
{
  for title in "${!EXISTING_MILESTONES[@]}"; do
    [ -n "${TARGET_MILESTONES[$title]}" ] && continue
    
    local existing=${EXISTING_MILESTONES[$title]}
    local number=$(jq -r .number <<< "$existing")
    local state=$(jq -r .state <<< "$existing")

    if [[ "$state" == "open" ]]; then
      echo "🚫 Closing obsolete milestone: \"$title\""
      call_github_api PATCH "/repos/$GITHUB_REPOSITORY/milestones/$number" \
        '{"state":"closed"}'
    fi
  done
}
purge_obsolete_milestones

echo "✅ Synchronization completed successfully"
