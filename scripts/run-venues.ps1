param()
$Root = (Resolve-Path "$PSScriptRoot/..\").Path
Write-Host "Starting Venue_A, Venue_B, Venue_C..."
$p1 = Start-Process "dotnet" -ArgumentList "run", "--project", "$Root/Venue_A/Venue_A.csproj" -PassThru
$p2 = Start-Process "dotnet" -ArgumentList "run", "--project", "$Root/Venue_B/Venue_B.csproj" -PassThru
$p3 = Start-Process "dotnet" -ArgumentList "run", "--project", "$Root/Venue_C/Venue_C.csproj" -PassThru
try {
    Wait-Process -Id $p1.Id, $p2.Id, $p3.Id
} finally {
    Write-Host "Stopping venues..."
    $p1 | Stop-Process -ErrorAction SilentlyContinue
    $p2 | Stop-Process -ErrorAction SilentlyContinue
    $p3 | Stop-Process -ErrorAction SilentlyContinue
}