$ROOT_DIR = git rev-parse --show-toplevel

if (-not (Test-Path $ROOT_DIR)) {
    Write-Error "Failed to get root directory"
    exit 1
}

Set-Location $ROOT_DIR

dotnet dotnet-svcutil "$env:ZHR_BASE_URL/sap/epr/deltas-wsdl" `
    -d "src/Pessoas.Integracao.Sync/Application/ZhrModels/Deltas/Generated/" `
    -o "DeltasService" `
    --namespace "*,Pessoas.Integracao.Sync.Application.ZhrModels.Deltas"