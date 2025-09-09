using Microsoft.Playwright;

namespace LoyaltyApi.Tests.E2E;

public static class PlaywrightConfig
{
    public static class BrowserOptions
    {
        public static BrowserNewContextOptions DefaultContextOptions => new()
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
            IgnoreHTTPSErrors = true,
            Locale = "en-US",
            TimezoneId = "America/New_York"
        };

        public static BrowserTypeLaunchOptions DefaultLaunchOptions => new()
        {
            Headless = true, // Set to false for debugging
            SlowMo = 0, // Add delay between actions for debugging
            Timeout = 30000
        };
    }

    public static class ApiDefaults
    {
        public const string BaseUrl = "https://localhost:5152";
        public const string ApiKey = "1f8b3f2a-6c71-4ed9-9db5-b1dca84dbe61";
        public const int RequestTimeout = 30000;
    }
}
