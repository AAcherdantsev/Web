#!/usr/bin/env bash
#
# Builds the projects affected by the changes since a base ref, and runs the tests of the affected test projects.
# Used by the pull request check, and can be run locally.
#
# Usage: scripts/run-affected-tests.sh <base-ref>
#   e.g. scripts/run-affected-tests.sh origin/main
#
# Set UNCOMMITTED=All to also include uncommitted changes in the working tree (default: None, committed changes only).
set -euo pipefail

BASE_REF="${1:?Usage: $0 <base-ref>}"
UNCOMMITTED="${UNCOMMITTED:-None}"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT_DIR="$(mktemp -d)"
trap 'rm -rf "$OUTPUT_DIR"' EXIT

cd "$ROOT"

dotnet tool restore
dotnet restore Web.slnx

# dotnet-affected exits with code 166 when nothing is affected, and writes affected.json otherwise.
# Any other non-zero code is a real failure, such as an unknown base ref.
NO_AFFECTED_EXIT_CODE=166
set +e
dotnet tool run dotnet-affected --from "$BASE_REF" --uncommitted "$UNCOMMITTED" \
    --format json --output-dir "$OUTPUT_DIR" --output-name affected >/dev/null
status=$?
set -e

if [ "$status" -eq "$NO_AFFECTED_EXIT_CODE" ]; then
    echo "No projects are affected by the changes since $BASE_REF. Nothing to build or test."
    exit 0
fi
if [ "$status" -ne 0 ]; then
    echo "dotnet-affected failed with exit code $status (is $BASE_REF a valid ref?)" >&2
    exit "$status"
fi

AFFECTED_FILE="$OUTPUT_DIR/affected.json"

echo "Affected projects:"
jq -r '.[] | "  - \(.Name)"' "$AFFECTED_FILE"

# Build every affected project, including the non-test ones (for example the AppHost)
jq -r '.[].FilePath' "$AFFECTED_FILE" | while IFS= read -r project; do
    dotnet build "$project" --no-restore --nologo --verbosity quiet
done

# Run the tests of the affected test projects. Every project runs, and the script fails at the end if any failed.
TEST_PROJECTS=$(jq -r '.[] | select(.Name | endswith(".Tests")) | .FilePath' "$AFFECTED_FILE")

if [ -z "$TEST_PROJECTS" ]; then
    echo "No affected test projects."
    exit 0
fi

failed=0
while IFS= read -r project; do
    echo "Testing $(basename "$project" .csproj)"
    dotnet test "$project" --no-build --nologo --verbosity normal || failed=1
done <<< "$TEST_PROJECTS"

exit "$failed"
