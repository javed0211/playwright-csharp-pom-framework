#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"

profile="${1:-local}"
filter="${2:-}"

args=(test PlaywrightPom.sln -c Release --settings "runsettings/${profile}.runsettings")
if [[ -n "$filter" ]]; then
  args+=(--filter "$filter")
fi

dotnet "${args[@]}"
