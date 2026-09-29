## Summary

-

## Test plan

- [ ] `dotnet test tests/PlaywrightPom.Core.Tests -c Release`
- [ ] `dotnet test tests/PlaywrightPom.Api.Tests -c Release --settings runsettings/ci.runsettings`
- [ ] Chromium web suite, or `TestCategory=Smoke` when the change is UI-only
- [ ] HTML report generated under `artifacts/reports`
