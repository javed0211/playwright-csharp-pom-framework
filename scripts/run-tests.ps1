param(
    [string]$Profile = "local",
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$dotnetArgs = @("test", "PlaywrightPom.sln", "-c", "Release", "--settings", "runsettings/$Profile.runsettings")
if ($Filter) {
    $dotnetArgs += @("--filter", $Filter)
}

dotnet @dotnetArgs
