#!/bin/sh

dotnet dotnet-sonarscanner begin \
          /key:pessoas-integracao \
          /n:"Plataforma de Integração de Pessoas" \
          /d:sonar.token=$SONARQUBE_CI_TOKEN \
          /d:sonar.host.url=$SONAR_HOST_URL \
          /d:sonar.cs.vscoveragexml.reportsPaths=coverage.xml \
          /d:sonar.exclusions="**/.devcontainer/**,**/Migrations/**,**/bin/**,**/obj/**,**/Generated/**" \
          /d:sonar.scanner.scanAll=false

dotnet build --no-incremental

dotnet dotnet-coverage collect 'dotnet test' \
    -f xml \
    -o 'coverage.xml'

dotnet dotnet-sonarscanner end /d:sonar.token=$SONARQUBE_CI_TOKEN
