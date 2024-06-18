#!/bin/bash

# Set the .git directory path
GIT_DIR=.git

# Read the current branch name from HEAD
GIT_HEAD=$(cat ${GIT_DIR}/HEAD)
GIT_BRANCH=${GIT_HEAD:16}

# Read the latest commit hash from the current branch ref
GIT_COMMIT=$(cat ${GIT_DIR}/refs/heads/${GIT_BRANCH})

# Create the Resources directory if it doesn't exist
mkdir -p "Assets/YourPlugin/Resources"

# Write the Git information to a text file
echo "Branch: ${GIT_BRANCH} | Commit: ${GIT_COMMIT}" > Assets/YourPlugin/Resources/git_info.txt

echo "Git info generated successfully."