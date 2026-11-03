# ============================================================
# start-backend.ps1  (DEVELOPMENT)
# Ports: Auth=5001, Product=5002, Order=5003, Gateway=5173
# DB   : saksham  (172.16.0.30)
# ============================================================

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# Stop any existing/orphan instances first so ports are 100% free
$servicesToStop = @("Auth.Api", "Product.Api", "Order.Api", "ApiGateway")
foreach ($name in $servicesToStop) {
    Stop-Process -Name $name -Force -ErrorAction SilentlyContinue
}

function Start-ServiceInstance {
    param(
        [string]$Name,
        [string]$ProjectPath,
        [int]$Port
    )
    Write-Host "  Starting $Name on port $Port ..." -ForegroundColor Cyan
    Start-Process powershell -WorkingDirectory "$ProjectPath" -ArgumentList @(
        "-NoExit",
        "-Command",
        "dotnet run --launch-profile http"
    ) -WindowStyle Normal
}

Write-Host ""
Write-Host "======================================================" -ForegroundColor Yellow
Write-Host "  AURA MART - DEVELOPMENT Environment Startup" -ForegroundColor Yellow
Write-Host "======================================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "Building solution first ..." -ForegroundColor Gray
dotnet build "$root\EcomSolution.sln" -c Debug --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed - aborting." -ForegroundColor Red
    exit 1
}
Write-Host "Build succeeded." -ForegroundColor Green
Write-Host ""

Start-ServiceInstance "Auth.Api"    "$root\src\Services\Auth\Auth.Api"       5001
Start-Sleep -Seconds 1
Start-ServiceInstance "Product.Api" "$root\src\Services\Product\Product.Api" 5002
Start-Sleep -Seconds 1
Start-ServiceInstance "Order.Api"   "$root\src\Services\Order\Order.Api"     5003
Start-Sleep -Seconds 1
Start-ServiceInstance "ApiGateway"  "$root\src\Gateways\ApiGateway"          5173

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
