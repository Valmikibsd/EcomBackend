# ============================================================
# start-backend.ps1  (DEVELOPMENT)
# Ports: Auth=5001, Product=5002, Order=5003, Gateway=5173
# DB   : saksham  (172.16.0.30)
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
        "Set-Location '$ProjectPath'; dotnet run --launch-profile http --no-build 2>&1"
    ) -WindowStyle Normal
}

Write-Host ""
Write-Host "======================================================" -ForegroundColor Yellow
Write-Host "  AURA MART — DEVELOPMENT Environment Startup" -ForegroundColor Yellow
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

Start-Service "Auth.Api"    "$root\src\Services\Auth\Auth.Api"       5001
Start-Sleep -Seconds 1
Start-Service "Product.Api" "$root\src\Services\Product\Product.Api" 5002
Start-Sleep -Seconds 1
Start-Service "Order.Api"   "$root\src\Services\Order\Order.Api"     5003
Start-Sleep -Seconds 1
Start-Service "ApiGateway"  "$root\src\Gateways\ApiGateway"          5173

Write-Host ""
Write-Host "======================================================" -ForegroundColor Green
Write-Host "  All services launching in separate windows." -ForegroundColor Green
Write-Host ""
Write-Host "  Gateway  : http://localhost:5173" -ForegroundColor White
Write-Host "  Auth     : http://localhost:5001/swagger" -ForegroundColor Gray
Write-Host "  Product  : http://localhost:5002/swagger" -ForegroundColor Gray
Write-Host "  Order    : http://localhost:5003/swagger" -ForegroundColor Gray
Write-Host ""
Write-Host "  Frontend : cd ecom-frontend && npm start" -ForegroundColor Cyan
Write-Host "             (serves on http://localhost:4200)" -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Green
Write-Host ""
