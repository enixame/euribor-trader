#!/usr/bin/env bash
set -euo pipefail
SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
ROOT_DIR=$(cd "$SCRIPT_DIR/.." && pwd)
echo "Starting Venue_A, Venue_B, Venue_C..."
dotnet run --project "$ROOT_DIR/Venue_A/Venue_A.csproj" &
PID_A=$!
dotnet run --project "$ROOT_DIR/Venue_B/Venue_B.csproj" &
PID_B=$!
dotnet run --project "$ROOT_DIR/Venue_C/Venue_C.csproj" &
PID_C=$!
trap 'echo "Stopping venues..."; kill $PID_A $PID_B $PID_C' INT TERM
wait