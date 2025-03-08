#!/bin/bash

REPO_OWNER="perkify"
REPO_NAME="perkify"
YAML_FILE="milestones.yml"

# Fetch latest milestones from Github
EXISTING_MILESTONES=$(gh api "/repos/$REPO_OWNER/$REPO_NAME/milestones?state=all" --jq '.[] | {title: .title, number: .number, state: .state}')

# Extract target milestones from YAML
TARGET_MILESTONES=$(yq e '.milestones[] | .title' "$YAML_FILE")

# Go through each milestone in YAML
while read -r milestone; do
  title=$(yq e '.title' <<< "$milestone")
  description=$(yq e '.description' <<< "$milestone")
  due_on=$(yq e '.due_on' <<< "$milestone")
  state=$(yq e '.state' <<< "$milestone")

  # Check if milestone already exists
  existing=$(jq -r --arg title "$title" 'select(.title == $title)' <<< "$EXISTING_MILESTONES")
  if [ -z "$existing" ]; then
    # Create new milestone
    gh api -X POST "/repos/$REPO_OWNER/$REPO_NAME/milestones" \
      -f title="$title" \
      -f description="$description" \
      -f due_on="$due_on" \
      -f state="$state"
    echo "Created milestone: $title"
  else
    # Update existing milestone
    number=$(jq -r '.number' <<< "$existing")
    gh api -X PATCH "/repos/$REPO_OWNER/$REPO_NAME/milestones/$number" \
      -f title="$title" \
      -f description="$description" \
      -f due_on="$due_on" \
      -f state="$state"
    echo "Updated milestone: $title"
  fi
done < <(yq e '.milestones[]' "$YAML_FILE")

# Close milestones that are not in YAML
echo "$EXISTING_MILESTONES" | jq -r '.title' | while read -r title; do
  if ! grep -q "$title" <<< "$TARGET_MILESTONES"; then
    number=$(jq -r --arg title "$title" 'select(.title == $title) | .number' <<< "$EXISTING_MILESTONES")
    state=$(jq -r --arg title "$title" 'select(.title == $title) | .state' <<< "$EXISTING_MILESTONES")
    if [ "$state" == "open" ]; then
      gh api -X PATCH "/repos/$REPO_OWNER/$REPO_NAME/milestones/$number" \
        -f state="closed"
      echo "Closed milestone: $title"
    fi
  fi
done