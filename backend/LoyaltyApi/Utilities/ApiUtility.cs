using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LoyaltyApi.Config;
using LoyaltyApi.Exceptions;
using LoyaltyApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

// CNO is integer
namespace LoyaltyApi.Utilities
{
    public class ApiUtility(IOptions<API> apiOptions,
    ILogger<ApiUtility> logger,
    IHttpClientFactory httpClientFactory,
    ParserUtility parserUtility)
    {
        public async Task<string> GetApiKey(string restaurantId)
        {
            var client = httpClientFactory.CreateClient("ApiClient");
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase);
            var isTesting = string.Equals(envName, "Testing", StringComparison.OrdinalIgnoreCase);

            logger.LogInformation("Getting API key for restaurantId: {restaurantId}, Environment: {envName}", restaurantId, envName);
            logger.LogInformation("API OPTIONS: {UserId}, {Password}, {BaseUrl}", apiOptions.Value.UserId, apiOptions.Value.Password, apiOptions.Value.BaseUrl);

            var body = new
            {
                acc = "202",
                usrid = "mcs",
                pass = "AN#$2025",
                // lng = "string",
                // thm = "string",
                // macid = "string",
                // fireBaseID = "string",
                // msg = "string",
                // auth = 0,
                // srcapp = "string",
                // srcver = 0
            };
            string jsonBody = JsonSerializer.Serialize(body);
            StringContent content = new(jsonBody, Encoding.UTF8, "application/json");
            var result = await client.PostAsync($"http://192.168.1.51:5000/api/CHKUSR", content);
            string responseContent = await result.Content.ReadAsStringAsync();
            if (!result.IsSuccessStatusCode || responseContent.Replace(" ", "").Contains("ERR"))
            {
                logger.LogError("Failed to get API key. Status Code: {statusCode}, Response: {response}", result.StatusCode, responseContent);
                throw new ApiUtilityException($"Operation Failed: {responseContent}");
            }

            logger.LogInformation("Request made to get ApiKey. Response Status Code: {statusCode}", result.StatusCode);
            return await result.Content.ReadAsStringAsync();
        }

        public async Task<string> GenerateVoucher(Voucher voucher, Restaurant restaurant, string apiKey)
        {
            var client = httpClientFactory.CreateClient("ApiClient");
            var body = new
            {
                DTL = new[]
                {
                    new
                        {
                            VOCHNO = 1,// This is constant do not change it
                            VOCHVAL = voucher.Value,
                            EXPDT = voucher.DateOfCreation.AddMinutes(restaurant.VoucherLifeTime).ToString("yyyy-MM-dd HH:mm"),
                        }
                },
                CNO = voucher.CustomerId,
                CCODE = "C"
            };
            logger.LogCritical(body.DTL.First().EXPDT);
            string jsonBody = JsonSerializer.Serialize(body);
            StringContent content = new(jsonBody, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("XApiKey", apiKey);
            var result = await client.PostAsync($"http://localhost:5021/api/HISCMD/ADDVOC", content);
            string responseContent = await result.Content.ReadAsStringAsync();
            logger.LogInformation("Request made to generate voucher. Response Message: {message}", responseContent);

            if (responseContent.Replace(" ", "").Contains("ERR") || !result.IsSuccessStatusCode)
                throw new ApiUtilityException($"Request to create voucher failed with message: {responseContent}");

            var responseObject = JsonSerializer.Deserialize<List<String>>(responseContent) ?? throw new HttpRequestException("Request to create voucher failed");
            
            return responseObject.First();
        }
        public async Task<User?> GetUserAsync(User user, string apiKey)
        {
            var client = httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Add("XApiKey", apiKey);
            string url = $"http://192.168.1.50:5001/api/concmd/GETCON/C/{user.PhoneNumber ?? user.Email ?? user.Id.ToString() ?? throw new ArgumentException("Phone number or email is missing")}";
            var result = await client.GetAsync(url);
            var json = await result.Content.ReadAsStringAsync();
            logger.LogInformation("Request made to get user. Response Message: .{message}.", json.ToString());

            if (json.ToString().Replace(" ", "").Contains("ERR") || json.IsNullOrEmpty() || !result.IsSuccessStatusCode)
            {
                logger.LogWarning("no user found");
                return null;
            }

            var userJson = JsonSerializer.Deserialize<JsonElement>(json);
            User? createdUser = new()
            {
                Id = userJson.GetProperty("CNO").GetInt32(),                   // Mapping "CNO" to User.Id
                PhoneNumber = userJson.GetProperty("TEL1").GetString()!,       // Mapping "TEL1" to User.PhoneNumber
                Email = userJson.GetProperty("EMAIL").GetString()!,            // Mapping "EMAIL" to User.Email
                Name = userJson.GetProperty("CNAME").GetString()!,             // Mapping "CNAME" to User.Name
                RestaurantId = user.RestaurantId,                                   // Use the passed restaurantId
            };
            return createdUser;
        }
        public async Task<User> CreateUserAsync(User user, string apiKey)
        {
            var client = httpClientFactory.CreateClient("ApiClient");
            var body = new
            {
                CCODE = "C", // Constant For Customer
                CON = "1", // es2al sayed bokra ya dika 
                CNO = "0", // 0 if New Customer else Check The Cno if Exist Update Else Insert
                CNAME = user.Name,
                FORDES = user.Name, // foreign desc
                TEL1 = user.PhoneNumber ?? "",
                TEL2 = "",
                EMAIL = user.Email,
                EMAIL1 = "",
                IMG = new
                {
                    Base64Data = ""
                }
            };
            // Serialize the body object to JSON
            string jsonBody = JsonSerializer.Serialize(body);

            logger.LogInformation("here is the api key, {apiKey}", apiKey);

            client.DefaultRequestHeaders.Add("XApiKey", apiKey);

            // Create a StringContent object with the JSON and specify the content type
            StringContent content = new(jsonBody, Encoding.UTF8, "application/json");

            // Send the POST request
            HttpResponseMessage response = await client.PostAsync($"http://192.168.1.50:5001/api/CONCMD/ADDCON", content);

            logger.LogInformation("Request made to create user. Response Status Code: {statusCode}", response.StatusCode);

            // Check the response status and handle accordingly
            string message = await response.Content.ReadAsStringAsync();
            var result = parserUtility.UserParser(message);
            dynamic dynamicResult = result;
            bool isSuccess = dynamicResult.success;

            if (isSuccess)
            {
                logger.LogInformation("User created with customer Number {customerId}", (string)dynamicResult.customerId.ToString());
                user.Id = dynamicResult.customerId;
                return user;
            }
            else
            {
                logger.LogWarning("User creation failed with message: {message}", message);
                throw new ApiUtilityException($"Request to create user failed with message: {message}");
            }
        }
        public async Task<User> UpdateUserAsync(User user, string apiKey)
        {
            var client = httpClientFactory.CreateClient("ApiClient");
            var body = new
            {
                CCODE = "C", // Constant For Customer 
                CNO = user.Id.ToString(), // 0 if New Customer else Check The Cno if Exist Update Else Insert
                CNAME = user.Name,
                FORDES = user.Name, // foreign desc
                TEL1 = user.PhoneNumber,
                TEL2 = "",
                EMAIL = user.Email,
                EMAIL1 = "",
                IMG = new
                {
                    Base64Data = ""
                }
            };
            // Serialize the body object to JSON
            string jsonBody = JsonSerializer.Serialize(body);

            client.DefaultRequestHeaders.Add("XApiKey", apiKey);

            // Create a StringContent object with the JSON and specify the content type
            StringContent content = new(jsonBody, Encoding.UTF8, "application/json");

            // Send the POST request
            HttpResponseMessage response = await client.PostAsync($"http://192.168.1.50:5001/api/CONCMD/ADDCON", content);
            string message = await response.Content.ReadAsStringAsync();

            if (message.Replace(" ", "").Contains("ERR") || !response.IsSuccessStatusCode)
                throw new ApiUtilityException($"Request to update user failed: {message}");

            logger.LogInformation("Request made to update user. Response Message: {message}", message);
            return user;
        }
    }
}