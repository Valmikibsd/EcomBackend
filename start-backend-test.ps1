# ============================================================
# start-backend-test.ps1
# Starts all 4 backend services in TEST environment
# Ports: Auth=6001, Product=6002, Order=6003, Gateway=6173
# DB   : saksham_test  (172.16.0.30)
# ============================================================

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

function Start-Service {
    param(
        [string]$Name,
        [string]$ProjectPath,
        [int]$Port
    )
    Write-Host "  Starting $Name on port $Port ..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList @(
        "-NoExit",
        "-Command",
        "Set-Location '$ProjectPath'; `$env:ASPNETCORE_ENVIRONMENT='Test'; dotnet run --launch-profile Test --no-build 2>&1"
    ) -WindowStyle Normal
}

Write-Host ""
Write-Host "======================================================" -ForegroundColor Yellow
Write-Host "  AURA MART — TEST Environment Startup" -ForegroundColor Yellow
Write-Host "======================================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "Building solution first ..." -ForegroundColor Gray
dotnet build "$root\EcomSolution.sln" -c Debug --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed — aborting." -ForegroundColor Red
    exit 1
}
Write-Host "Build succeeded." -ForegroundColor Green
Write-Host ""

Start-Service "Auth.Api     (TEST)" "$root\src\Services\Auth\Auth.Api"     6001
Start-Sleep -Seconds 1
Start-Service "Product.Api  (TEST)" "$root\src\Services\Product\Product.Api" 6002
Start-Sleep -Seconds 1
Start-Service "Order.Api    (TEST)" "$root\src\Services\Order\Order.Api"    6003
Start-Sleep -Seconds 1
Start-Service "ApiGateway   (TEST)" "$root\src\Gateways\ApiGateway"         6173

Write-Host ""
Write-Host "======================================================" -ForegroundColor Green
Write-Host "  All services launching in separate windows." -ForegroundColor Green
Write-Host ""
Write-Host "  Gateway  : http://localhost:6173" -ForegroundColor White
Write-Host "  Auth     : http://localhost:6001/swagger" -ForegroundColor Gray
Write-Host "  Product  : http://localhost:6002/swagger" -ForegroundColor Gray
Write-Host "  Order    : http://localhost:6003/swagger" -ForegroundColor Gray
Write-Host ""
Write-Host "  Frontend : cd ecom-frontend && npm run start:test" -ForegroundColor Cyan
Write-Host "             (serves on http://localhost:4201)" -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Green
Write-Host ""
