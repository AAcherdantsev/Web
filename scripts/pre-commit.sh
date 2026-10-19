#!/usr/bin/env bash
#
# Runs before every commit (see .husky/task-runner.json). Formats the code, regenerates THIRD-PARTY-NOTICES.md,
# and stages the results, so that the commit contains formatted code and up-to-date notices.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

echo "Formatting the code..."
dotnet format whitespace Web.slnx

echo "Regenerating THIRD-PARTY-NOTICES.md..."
bash scripts/generate-third-party-notices.sh

# Re-stage the files that were already staged, so that the formatted version is the one committed.
# Files that were not staged stay unstaged, and the formatting changes to them are left in the working tree.
git diff --name-only --cached --diff-filter=ACMR -z | xargs -0 -r git add --
git add THIRD-PARTY-NOTICES.md
