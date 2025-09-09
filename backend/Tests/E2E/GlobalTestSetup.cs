using NUnit.Framework;

[assembly: LevelOfParallelism(1)]

namespace LoyaltyApi.Tests.E2E;

// Global test settings
[SetUpFixture]
public class GlobalTestSetup
{
    [OneTimeSetUp]
    public void GlobalSetup()
    {
        // Install Playwright browsers if not already installed
        Microsoft.Playwright.Program.Main(new[] { "install" });
        
        // Any other global setup
        Console.WriteLine("Starting E2E Tests...");
    }

    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        Console.WriteLine("E2E Tests completed.");
    }
}
