using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace LoyaltyApi.Tests.E2E;

[TestFixture]
public class BaseTest : PageTest
{
    protected string BaseUrl = "http://localhost:5152";  // Changed from https to http
    protected string ApiKey = "1f8b3f2a-6c71-4ed9-9db5-b1dca84dbe61";

    [SetUp]
    public async Task Setup()
    {
        // Set up common configurations
        await Page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
        {
            ["X-ApiKey"] = ApiKey
        });

        // Set default timeout
        Page.SetDefaultTimeout(30000);

        // Navigate to base URL for API tests
        // await Page.GotoAsync($"{BaseUrl}/swagger/index.html");
    }

    [TearDown]
    public async Task Cleanup()
    {
        // Clean up any test data if needed
        await Page.CloseAsync();
    }

    protected async Task<IAPIResponse> MakeApiRequest(string method, string endpoint, object? body = null)
    {
        var requestOptions = new APIRequestContextOptions
        {
            Method = method,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                ["X-ApiKey"] = ApiKey
            }
        };

        if (body != null)
        {
            if (body is string bodyStr)
            {
                requestOptions.DataString = bodyStr;
            }
            else
            {
                requestOptions.DataObject = body;
            }
        }

        return await Page.APIRequest.FetchAsync($"{BaseUrl}{endpoint}", requestOptions);
    }
    protected async Task<IAPIResponse> MakeAuthenticatedApiRequest(string method, string endpoint, string accessToken, object? body = null)
    {
        var options = new APIRequestContextOptions
        {
            Method = method,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                ["Authorization"] = $"Bearer {accessToken}"
            }
        };

        if (body != null)
        {
            if (body is string stringBody)
            {
                options.DataString = stringBody;
            }
            else
            {
                options.DataObject = body;
            }
        }

 return await Page.APIRequest.FetchAsync($"{BaseUrl}{endpoint}", options);    }
}
