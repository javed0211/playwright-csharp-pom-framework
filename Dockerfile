FROM mcr.microsoft.com/playwright/dotnet:v1.63.0-noble

WORKDIR /src
COPY . .
RUN dotnet restore PlaywrightPom.sln \
    && dotnet build PlaywrightPom.sln -c Release --no-restore

# The image has .NET 10 only and Playwright's bundled Chromium, not Google Chrome: the net9.0 tests run on the
# newer runtime, and the configured "chrome" channel is cleared.
ENV NEXUS_ENVIRONMENT=CI \
    DOTNET_ROLL_FORWARD=Major \
    NEXUS_Run__Channel= \
    HEADED=0 \
    BROWSER=chromium \
    CI=true

CMD ["dotnet", "test", "PlaywrightPom.sln", "-c", "Release", "--no-build", "--settings", "runsettings/ci.runsettings", "--logger", "trx;LogFileName=docker.trx"]
