@echo off
REM E2E Test Runner Script for Rock Loyalty System (Windows)

echo 🚀 Starting E2E Tests for Rock Loyalty System

REM Check if .NET is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ .NET is not installed. Please install .NET 8.0 or higher.
    exit /b 1
)

REM Navigate to the Tests directory
cd /d "%~dp0"

echo 📦 Restoring test packages...
dotnet restore

echo 🔨 Building the test project...
dotnet build --configuration Debug

echo 🚀 Starting the API server in background...
cd /d "%~dp0\..\LoyaltyApi"
start /B dotnet run --launch-profile https
cd /d "%~dp0"
timeout /t 15 /nobreak > nul

echo ⏳ Waiting for API to start...
timeout /t 5 /nobreak > nul

echo 🧪 Installing Playwright browsers...
dotnet exec microsoft.playwright.dll install
pwsh -Command "dotnet run --project . --framework net8.0 -- install"

echo 🧪 Running E2E Tests...

REM Run tests in order
echo 🔐 Running Authentication Tests...
dotnet test --filter "FullyQualifiedName~AuthenticationTests" --logger "console;verbosity=detailed"

echo 🌐 Running OAuth2 Tests...
dotnet test --filter "FullyQualifiedName~OAuth2" --logger "console;verbosity=detailed"

echo 👤 Running User Function Tests...
dotnet test --filter "FullyQualifiedName~UserFunctionTests" --logger "console;verbosity=detailed"

echo 🎫 Running Voucher Tests...
dotnet test --filter "FullyQualifiedName~VoucherTests" --logger "console;verbosity=detailed"

echo 💰 Running Credit Points Tests...
dotnet test --filter "FullyQualifiedName~CreditPointsTests" --logger "console;verbosity=detailed"

echo 🏪 Running Restaurant Tests...
dotnet test --filter "FullyQualifiedName~RestaurantTests" --logger "console;verbosity=detailed"

REM Run all tests
echo 🧪 Running All E2E Tests...
dotnet test --logger "console;verbosity=detailed" --logger "trx;LogFileName=e2e-results.trx"

echo 🛑 Stopping API server...
taskkill /F /IM dotnet.exe /T >nul 2>nul

echo ✅ E2E Tests completed!
echo 📊 Test results available in TestResults/e2e-results.trx

pause
