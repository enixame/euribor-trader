<##>
## Runs the WPF client.  Ensure venues are running before starting the client.
<##>

$root = Split-Path -Parent $PSScriptRoot
Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$root/Trading.Client.Wpf`"" -Wait