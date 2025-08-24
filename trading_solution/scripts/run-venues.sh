#!/usr/bin/env bash
# Runs the three venue console services concurrently.  Each service writes its
# own output to the console.  Use Ctrl+C to terminate all services.

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="${SCRIPT_DIR}/.."

dotnet run --project "${ROOT_DIR}/Venue_A" &
PID_A=$!
dotnet run --project "${ROOT_DIR}/Venue_B" &
PID_B=$!
dotnet run --project "${ROOT_DIR}/Venue_C" &
PID_C=$!

trap "echo 'Stopping venues...'; kill $PID_A $PID_B $PID_C" SIGINT SIGTERM

wait $PID_A $PID_B $PID_C