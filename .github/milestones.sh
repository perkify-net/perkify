#!/bin/bash

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
echo "  - Config file: $YAML_FILE"
echo "  - Dry run: $DRY_RUN"

# Fetch existing milestones from Github
EXISTING_MILESTONES=$(gh api "/repos/$REPO_OWNER/$REPO_NAME/milestones?state=all" --jq '.[] | {title: .title, number: .number, state: .state}')

# Extract target milestones from YAML
TARGET_MILESTONES=$(yq e '.milestones[] | .title' "$MILESTONES_YAML_FILE")

# Go through each milestone in YAML file
while read -r milestone; do
  title=$(yq e '.title' <<< "$milestone")
  description=$(yq e '.description' <<< "$milestone")
  due_on=$(yq e '.due_on' <<< "$milestone")
  state=$(yq e '.state' <<< "$milestone")

  # Check if milestone exists
  existing=$(jq -r --arg title "$title" 'select(.title == $title)' <<< "$EXISTING_MILESTONES")
  
  if [ -z "$existing" ]; then
    echo "→ Create new milestone: $title"
    call_github_api POST "/repos/$REPO_OWNER/$REPO_NAME/milestones" \
      "-f title='$title' \
       -f description='$description' \
       -f due_on='$due_on' \
       -f state='$state'"
  else
    number=$(jq -r '.number' <<< "$existing")
    current_state=$(jq -r '.state' <<< "$existing")
    
    if [ "$current_state" != "$state" ] || [ "$DRY_RUN" = true ]; then
      echo "→ Update existing milestone: $title (state: $current_state → $state)"
      call_github_api PATCH "/repos/$REPO_OWNER/$REPO_NAME/milestones/$number" \
        "-f title='$title' \
         -f description='$description' \
         -f due_on='$due_on' \
         -f state='$state'"
    fi
  fi
done < <(yq e '.milestones[]' "$YAML_FILE")

# Close obsolete milestones
echo "$EXISTING_MILESTONES" | jq -r '.title' | while read -r title; do
  if ! yq e ".milestones[] | select(.title == \"$title\")" "$YAML_FILE" >/dev/null; then
    number=$(jq -r --arg title "$title" 'select(.title == $title) | .number' <<< "$EXISTING_MILESTONES")
    state=$(jq -r --arg title "$title" 'select(.title == $title) | .state' <<< "$EXISTING_MILESTONES")
    
    if [ "$state" = "open" ]; then
      echo "→ Close obsolete milestone: $title"
      call_github_api PATCH "/repos/$REPO_OWNER/$REPO_NAME/milestones/$number" \
        "-f state='closed'"
    fi
  fi
done