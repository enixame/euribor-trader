<##>
## Runs the three venue console services concurrently in PowerShell.
## Each service writes its own output to the console.
## Press Ctrl+C to terminate all services.
<##>

$root = Split-Path -Parent $PSScriptRoot
$procs = @()
$procs += Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$root/Venue_A`"" -PassThru
$procs += Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$root/Venue_B`"" -PassThru
$procs += Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$root/Venue_C`"" -PassThru

Write-Host "Venues running. Press Ctrl+C to stop..."
try {
    Wait-Process -Id $procs.Id
} finally {
    foreach ($p in $procs) { if (!$p.HasExited) { $p.Kill() } }
}