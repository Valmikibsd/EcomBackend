# ============================================================
# stop-backend.ps1
# Stops all running instances of AURA MART backend services
# ============================================================

$processNames = @("Auth.Api", "Product.Api", "Order.Api", "ApiGateway")

Write-Host "Stopping backend services..." -ForegroundColor Cyan

foreach ($name in $processNames) {
    $procs = Get-Process -Name $name -ErrorAction SilentlyContinue
    if ($procs) {
        $procs | ForEach-Object {
            Write-Host "  Stopping $name (PID: $($_.Id))" -ForegroundColor Yellow
            Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
        }
    }
}

Write-Host "All backend processes stopped. DLL locks released." -ForegroundColor Green
