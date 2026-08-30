#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")"

# Load MSSQL_SA_PASSWORD from .env
if [ ! -f .env ]; then
  echo ".env not found. Run: cp .env.example .env" >&2
  exit 1
fi
export $(grep -v '^#' .env | xargs)

dotnet ef database update \
  --project MiraiShop.Infrastructure \
  --startup-project MiraiShop.Server \
  --connection "Server=localhost,1433;Database=MiraiShop;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True"
