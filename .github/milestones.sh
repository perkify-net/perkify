#!/bin/bash

# Detect if jq, yq & Github CLI is installed
command -v gh >/dev/null || { echo "ERROR: Please install gh: brew install gh"; exit 1; }
command -v jq >/dev/null || { echo "ERROR: Please install jq: brew install jq"; exit 1; }
command -v yq >/dev/null || { echo "ERROR: Please install yq: brew install yq"; exit 1; }

# Priority: CLI Args > Env Vars > Defaults
REPO_OWNER="${1:-${GITHUB_REPOSITORY_OWNER:-default_owner}}"
REPO_NAME="${2:-${GITHUB_REPOSITORY#*/}}"
MILESTONES_YAML_FILE="${3:-${MILESTONES_YAML_FILE:-milestones.yml}}"
DRY_RUN="${4:-${DRY_RUN:-false}}"
[[ "$DRY_RUN" = "true" ]] && DRY_RUN=true || DRY_RUN=false

# Wrapper for Github API call
call_github_api() {
  local method=$1
  local endpoint=$2
  local data=$3

  if [ "$DRY_RUN" = true ]; then
    echo "[DRY RUN] Would execute: gh api -X $method $endpoint $data"
  else
    gh api -X $method "$endpoint" $data
  fi
}

# Start syncing milestones
echo "Syncing with config:"
echo "  - Repository: $REPO_OWNER/$REPO_NAME"
echo "  - Config file: $MILESTONES_YAML_FILE"
echo "  - Dry run: $DRY_RUN"

# Fetch existing milestones from Github
declare -A EXISTING_MILESTONES
EXISTING_MILESTONES_JSON=$(gh api -q '.[] | @base64' "/repos/$REPO_OWNER/$REPO_NAME/milestones?state=all")
while IFS= read -r milestone; do
  title=$(echo "$milestone" | base64 --decode | jq -r .title)
  EXISTING_MILESTONES["$title"]=$(echo "$milestone" | base64 --decode)
done <<< "$EXISTING_MILESTONES_JSON"

# Extract target milestone titles from YAML
declare -A TARGET_MILESTONES
while IFS= read -r milestone; do
  title=$(echo "$milestone" | base64 --decode | jq -r .title)
  data=$(echo "$milestone" | base64 --decode)
  TARGET_MILESTONES["$title"]="$data"
done < <(yq -j '.milestones[] | @base64' "$CONFIG_FILE")

# Go through each milestone in YAML file
for title in "${!TARGET_MILESTONES[@]}"; do
  target_data=${TARGET_MILESTONES["$title"]}
  description=$(jq -r '.description // ""' <<< "$target_data")
  due_on=$(jq -r '.due_on // ""' <<< "$target_data")
  state=$(jq -r '.state // "open"' <<< "$target_data")

  request_data=$(jq -n \
    --arg desc "$description" \
    --arg due "$due_on" \
    --arg state "$state" \
    '{description: $desc, due_on: (if $due == "" then null else $due end), state: $state}')

  if [[ -n "${EXISTING_MILESTONES[$title]}" ]]; then
    number=$(jq -r .number <<< "${EXISTING_MILESTONES[$title]}")
    current_data=$(jq -c '{description, due_on, state}' <<< "${EXISTING_MILESTONES[$title]}")

    if ! diff <(jq -S . <<< "$request_data") <(jq -S . <<< "$current_data") &>/dev/null; then
      echo "🔄 Update existing milestone: $title"
      call_github_api PATCH "/repos/$REPO_OWNER/$REPO_NAME/milestones/$number" "$request_data"
    fi
  else
    echo "🆕 Create new milestone: $title"
    call_github_api POST "/repos/$REPO_OWNER/$REPO_NAME/milestones" "$(jq --arg title "$title" '. + {title: $title}' <<< "$request_data")"
  fi
done

# Close obsolete milestones
or title in "${!EXISTING_MILESTONES[@]}"; do
  if [[ -z "${TARGET_MILESTONES[$title]}" ]]; then
    number=$(jq -r .number <<< "${EXISTING_MILESTONES[$title]}")
    state=$(jq -r .state <<< "${EXISTING_MILESTONES[$title]}")
    
    if [[ "$state" == "open" ]]; then
      echo "🚫 关闭多余里程碑: $title"
      call_api PATCH "/repos/$REPO_OWNER/$REPO_NAME/milestones/$number" '{"state":"closed"}'
    fi
  fi
done
