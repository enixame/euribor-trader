#!/usr/bin/env bash
# Runs the WPF trading client.  Ensure venues are running before starting the client.

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="${SCRIPT_DIR}/.."
dotnet run --project "${ROOT_DIR}/Trading.Client.Wpf" "$@"