param()
$Root = (Resolve-Path "$PSScriptRoot/..\").Path
Write-Host "Starting trading client..."
dotnet run --project "$Root/Trading.Client.Wpf/Trading.Client.Wpf.csproj"