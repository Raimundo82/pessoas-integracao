#!/usr/bin/env bash
set -e

# -----------------------------
# Resolve script directory
# -----------------------------
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# All paths are relative to the script location
BASE_URL="http://esb-lb-soa.marinha.pt:8301"

BASE_OUTPUT_DIR="${SCRIPT_DIR}/src/Pessoas.Integracao.Worker/Infrastructure/Sigdn.Rh/Soap/Generated"
BASE_NAMESPACE="Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated"

SERVICES=(
  "Output|ZHR_EPR"
  "Deltas|ZHR_deltas_EPR"
  "Descodificadoras|ZHR_descodif_EPR"
)

echo "Running from script directory: ${SCRIPT_DIR}"
echo "Generating SIGDN-RH SOAP clients..."

for SERVICE in "${SERVICES[@]}"; do
  NAME="${SERVICE%%|*}"
  WSDL_PATH="${SERVICE##*|}"

  OUTPUT_DIR="${BASE_OUTPUT_DIR}/${NAME}"
  NAMESPACE="${BASE_NAMESPACE}.${NAME}"
  OUTPUT_FILE="${NAME}Service.cs"
  WSDL_URL="${BASE_URL}/${WSDL_PATH}?wsdl"

  echo " - Generating ${NAME}"
  mkdir -p "${OUTPUT_DIR}"

  dotnet dotnet-svcutil \
    "${WSDL_URL}" \
    -d "${OUTPUT_DIR}" \
    -o "${OUTPUT_FILE}" \
    --namespace "*,${NAMESPACE}"
done

echo "✔ Done."
