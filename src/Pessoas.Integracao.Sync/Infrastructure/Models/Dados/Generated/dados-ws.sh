#!/bin/bash

# Get the absolute path to the git root directory
ROOT_DIR=$(git rev-parse --show-toplevel)

cd "$ROOT_DIR" || { echo "Failed to change to root directory"; exit 1; }

dotnet dotnet-svcutil $ZHR_BASE_URL/ZHR_EPR?wsdl \
-d src/Pessoas.Integracao.Sync/Infrastructure/Models/Dados/Generated/ \
-o DadosService  \
--namespace "*,Pessoas.Integracao.Sync.Infrastructure.Models.Dados"