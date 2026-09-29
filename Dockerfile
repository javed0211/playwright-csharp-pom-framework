FROM mcr.microsoft.com/playwright/dotnet:v1.63.0-noble

WORKDIR /src
COPY . .
RUN dotnet restore PlaywrightPom.sln \
    && dotnet build PlaywrightPom.sln -c Release --no-restore

ENV NEXUS_ENVIRONMENT=CI \
    HEADED=0 \
    BROWSER=chromium \
    CI=true

CMD ["dotnet", "test", "PlaywrightPom.sln", "-c", "Release", "--no-build", "--settings", "runsettings/ci.runsettings", "--logger", "trx;LogFileName=docker.trx"]
