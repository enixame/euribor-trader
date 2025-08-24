#!/usr/bin/env bash
set -euo pipefail
SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
ROOT_DIR=$(cd "$SCRIPT_DIR/.." && pwd)
echo "Starting trading client..."
dotnet run --project "$ROOT_DIR/Trading.Client.Wpf/Trading.Client.Wpf.csproj"