# ============================================================
# publish-all.ps1
# Builds and publishes Backend + Frontend for TEST Deployment
# Output Directory: D:\ecom-frontendjwt\publish\
# ============================================================

$ErrorActionPreference = "Stop"
$solutionRoot = "D:\ecom-frontendjwt\ecom-backend"
$frontendRoot = "D:\ecom-frontendjwt\ecom-frontend"
$outputBase   = "D:\ecom-frontendjwt\publish"

Write-Host ""
Write-Host "========================================================" -ForegroundColor Yellow
Write-Host "  AURA MART - Publishing for TEST Server Deployment" -ForegroundColor Yellow
Write-Host "========================================================" -ForegroundColor Yellow
Write-Host ""

# Clean previous publish folder
if (Test-Path $outputBase) {
    Write-Host "Cleaning existing publish folder..." -ForegroundColor Gray
    Remove-Item -Path $outputBase -Recurse -Force
}
New-Item -ItemType Directory -Path "$outputBase\backend" -Force | Out-Null
New-Item -ItemType Directory -Path "$outputBase\frontend" -Force | Out-Null

# ------------------------------------------------------------
# 1. Publish Backend Services (.NET 9)
# ------------------------------------------------------------
$services = @(
    @{ Name = "ApiGateway";  Project = "$solutionRoot\src\Gateways\ApiGateway\ApiGateway.csproj" },
    @{ Name = "Auth.Api";   Project = "$solutionRoot\src\Services\Auth\Auth.Api\Auth.Api.csproj" },
    @{ Name = "Product.Api";Project = "$solutionRoot\src\Services\Product\Product.Api\Product.Api.csproj" },
    @{ Name = "Order.Api";  Project = "$solutionRoot\src\Services\Order\Order.Api\Order.Api.csproj" }
)

foreach ($svc in $services) {
    $outDir = "$outputBase\backend\$($svc.Name)"
    Write-Host "Publishing $($svc.Name) -> $outDir ..." -ForegroundColor Cyan
    dotnet publish $svc.Project -c Release -o $outDir --nologo -v q
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to publish $($svc.Name)" -ForegroundColor Red
        exit 1
    }
}

# Copy SQL migration scripts into publish folder for easy database setup
Write-Host "Copying SQL setup scripts..." -ForegroundColor Cyan
Copy-Item -Path "$solutionRoot\scripts" -Destination "$outputBase\sql-scripts" -Recurse -Force

# ------------------------------------------------------------
# 2. Publish Angular Frontend (test-env)
# ------------------------------------------------------------
Write-Host "Building Angular Frontend (test-env)..." -ForegroundColor Cyan
Set-Location $frontendRoot
npx ng build --configuration test-env

$frontendDist = "$frontendRoot\dist\ecom-frontend\browser"
if (Test-Path $frontendDist) {
    Copy-Item -Path "$frontendDist\*" -Destination "$outputBase\frontend" -Recurse -Force
} else {
    Write-Host "Could not find frontend dist directory at $frontendDist" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================================" -ForegroundColor Green
Write-Host "  PUBLISH COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
Write-Host ""
Write-Host "All deployment artifacts are located at:" -ForegroundColor White
Write-Host "  D:\ecom-frontendjwt\publish\" -ForegroundColor Yellow
Write-Host ""
Write-Host "Folder structure:" -ForegroundColor White
Write-Host "  publish\" -ForegroundColor White
Write-Host "    |-- backend\" -ForegroundColor White
Write-Host "    |     |-- ApiGateway\       (Port 6173 on Test)" -ForegroundColor Gray
Write-Host "    |     |-- Auth.Api\         (Port 6001 on Test)" -ForegroundColor Gray
Write-Host "    |     |-- Product.Api\      (Port 6002 on Test)" -ForegroundColor Gray
Write-Host "    |     |-- Order.Api\        (Port 6003 on Test)" -ForegroundColor Gray
Write-Host "    |-- frontend\               (Host in IIS/Nginx on Port 4201 or 80)" -ForegroundColor Gray
Write-Host "    |-- sql-scripts\            (01 to 04 SQL scripts to set up saksham_test DB)" -ForegroundColor Gray
Write-Host ""
