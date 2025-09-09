#!/bin/bash

# E2E Test Runner Script for Rock Loyalty System

echo "🚀 Starting E2E Tests for Rock Loyalty System"

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET is not installed. Please install .NET 8.0 or higher."
    exit 1
fi

# Navigate to the Tests directory
cd "$(dirname "$0")" || exit 1

echo "📦 Restoring test packages..."
dotnet restore

echo "🔨 Building the test project..."
dotnet build --configuration Debug

echo "🚀 Starting the API server in background..."
cd ../LoyaltyApi || exit 1
dotnet run --launch-profile https &
API_PID=$!
cd ../Tests || exit 1

# Wait for the API to start
echo "⏳ Waiting for API to start..."
sleep 15

# Check if API is running
if curl -k -s https://localhost:5152/swagger/index.html > /dev/null; then
    echo "✅ API is running on https://localhost:5152"
else
    echo "❌ API failed to start"
    kill $API_PID
    exit 1
fi

echo "🧪 Installing Playwright browsers..."
dotnet run --project . --framework net8.0 -- install

echo "🧪 Running E2E Tests..."

# Run tests in order
echo "🔐 Running Authentication Tests..."
dotnet test --filter "FullyQualifiedName~AuthenticationTests" --logger "console;verbosity=detailed"

echo "🌐 Running OAuth2 Tests..."
dotnet test --filter "FullyQualifiedName~OAuth2" --logger "console;verbosity=detailed"

echo "👤 Running User Function Tests..."
dotnet test --filter "FullyQualifiedName~UserFunctionTests" --logger "console;verbosity=detailed"

echo "🎫 Running Voucher Tests..."
dotnet test --filter "FullyQualifiedName~VoucherTests" --logger "console;verbosity=detailed"

echo "💰 Running Credit Points Tests..."
dotnet test --filter "FullyQualifiedName~CreditPointsTests" --logger "console;verbosity=detailed"

echo "🏪 Running Restaurant Tests..."
dotnet test --filter "FullyQualifiedName~RestaurantTests" --logger "console;verbosity=detailed"

# Run all tests
echo "🧪 Running All E2E Tests..."
dotnet test --logger "console;verbosity=detailed" --logger "trx;LogFileName=e2e-results.trx"

echo "🛑 Stopping API server..."
kill $API_PID

echo "✅ E2E Tests completed!"
echo "📊 Test results available in TestResults/e2e-results.trx"
