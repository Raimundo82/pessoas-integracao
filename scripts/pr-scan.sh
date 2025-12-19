#!/bin/sh

dotnet dotnet-sonarscanner begin \
          /key:sigdn-rh-stagging-api \
          /n:"RH Staging" \
          /d:sonar.token=$SONARQUBE_CI_TOKEN \
          /d:sonar.host.url=$SONAR_HOST_URL \
          /d:sonar.cs.vscoveragexml.reportsPaths=coverage.xml \
          /d:sonar.pullrequest.key=99 \
          /d:sonar.pullrequest.branch=test \
          /d:sonar.pullrequest.base=master \
          /d:sonar.exclusions="**/.devcontainer/**,**/Migrations/**,**/bin/**,**/obj/**" \
          /d:sonar.scanner.scanAll=false

dotnet build --no-incremental

dotnet dotnet-coverage collect 'dotnet test' \
    -f xml \
    -o 'coverage.xml'

dotnet dotnet-sonarscanner end /d:sonar.token=$SONARQUBE_CI_TOKEN
