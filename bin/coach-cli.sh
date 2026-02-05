#!/bin/bash
# Wrapper script for CoachCli

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec dotnet "$SCRIPT_DIR/coach-cli/CoachCli.dll" "$@"
