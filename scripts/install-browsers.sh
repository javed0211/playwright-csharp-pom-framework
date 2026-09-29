#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"

browser="${1:-chromium}"
dotnet build tests/PlaywrightPom.Web.Tests/PlaywrightPom.Web.Tests.csproj -c Release
out="$(find tests/PlaywrightPom.Web.Tests/bin/Release -type d -name net9.0 | head -n 1)"

if command -v pwsh >/dev/null 2>&1; then
  pwsh "$out/playwright.ps1" install "$browser"
  exit 0
fi

nodebin="$(find "$out/.playwright/node" -type f -name node | head -n 1)"
"$nodebin" "$out/.playwright/package/cli.js" install "$browser"
