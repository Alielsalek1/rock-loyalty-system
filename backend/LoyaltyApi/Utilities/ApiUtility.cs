using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using LoyaltyApi.Config;
using LoyaltyApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

// CNO is integer
namespace LoyaltyApi.Utilities
{
    public class ApiUtility(IOptions<API> apiOptions,
    ILogger<ApiUtility> logger)
    {
        public async Task<string> GetApiKey(string restaurantId)
        {
            using HttpClient client = new();
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isDevelopment = string.Equals(envName, "Development", StringComparison.OrdinalIgnoreCase);
            var isTesting = string.Equals(envName, "Testing", StringComparison.OrdinalIgnoreCase);

            logger.LogInformation("Getting API key for restaurantId: {restaurantId}, Environment: {envName}", restaurantId, envName);
            logger.LogInformation("API OPTIONS: {UserId}, {Password}, {BaseUrl}", apiOptions.Value.UserId, apiOptions.Value.Password, apiOptions.Value.BaseUrl);

            var body = new
            {
                acc = isDevelopment || isTesting ? "202" : restaurantId ?? throw new ArgumentException("Resturant ID is missing"), // change acc for different restaurants
                usrid = apiOptions.Value.UserId,
                pass = apiOptions.Value.Password,
                lng = "ENG",
                // SrcApp = "WEBAPP",
                // SrcVer = 1.00

            };
            string jsonBody = JsonSerializer.Serialize(body);
            StringContent content = new(jsonBody, Encoding.UTF8, "application/json");
            var result = await client.PostAsync("http://192.168.1.51:5000/api/CHKUSR", content);
            logger.LogInformation("Request made to get ApiKey. Response Status Code: {statusCode}", result.StatusCode);
            return await result.Content.ReadAsStringAsync();
        }

        public async Task<string> GenerateVoucher(Voucher voucher, Restaurant restaurant, string apiKey)
        {
            using HttpClient client = new();
            var body = new
            {
                DTL = new[]
                {
                    new
                        {
                            VOCHNO = 1,// This is constant do not change it
                            VOCHVAL = voucher.Value,
                            EXPDT = voucher.DateOfCreation.AddMinutes(restaurant.VoucherLifeTime).ToString("yyyy-MM-dd"),
                        }
                },
                CNO = voucher.CustomerId.ToString(),
                CCODE = "C"
            };
            logger.LogCritical(body.DTL.First().EXPDT);
            string jsonBody = JsonSerializer.Serialize(body);
            StringContent content = new(jsonBody, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("XApiKey", apiKey);
            var result = await client.PostAsync($"{apiOptions.Value.BaseUrl}/api/HISCMD/ADDVOC", content);
            var message = await result.Content.ReadAsStringAsync();
            logger.LogInformation("Request made to generate voucher. Response Message: {message}", message);
            if (message.Replace(" ", "").Contains("ERR"))
                throw new HttpRequestException($"Request to create user failed with message: {message}");
            string responseContent = await result.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<List<String>>(responseContent) ?? throw new HttpRequestException("Request to create user failed");
            return responseObject.First();
        }
        public async Task<User?> GetUserAsync(User user, string apiKey)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("XApiKey", apiKey);
            string url = $"{apiOptions.Value.BaseUrl}/api/concmd/GETCON/C/{user.PhoneNumber ?? user.Email ?? user.Id.ToString() ?? throw new ArgumentException("Phone number or email is missing")}";
            var result = await client.GetAsync(url);
            var json = await result.Content.ReadAsStringAsync();
            logger.LogInformation("Request made to get user. Response Message: .{message}.", json.ToString());

            if (json.ToString().Replace(" ", "").Contains("ERR") || json.IsNullOrEmpty())
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
        public async Task<User?> CreateUserAsync(User user, string apiKey)
        {
            using HttpClient client = new();
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
                IMG = ""
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
            if (message.Replace(" ", "").Contains("ERR"))
                throw new HttpRequestException($"Request to create user failed with message: {message}");
            Match match = Regex.Match(message, @"\d+");
            user.Id = int.Parse(match.Value);
            logger.LogInformation("Request made to create user with id: {Id} . Response Message: {message}", user.Id, message);
            return user;
        }
        public async Task<User> UpdateUserAsync(User user, string apiKey)
        {
            using HttpClient client = new();
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
            };
            // Serialize the body object to JSON
            string jsonBody = JsonSerializer.Serialize(body);

            client.DefaultRequestHeaders.Add("XApiKey", apiKey);

            // Create a StringContent object with the JSON and specify the content type
            StringContent content = new(jsonBody, Encoding.UTF8, "application/json");

            // Send the POST request
            HttpResponseMessage response = await client.PostAsync($"{apiOptions.Value.BaseUrl}/api/CONCMD/ADDCON", content);
            string message = await response.Content.ReadAsStringAsync();
            if (message.Replace(" ", "").Contains("ERR"))
                throw new HttpRequestException($"Request to create user failed with message: {message}");
            logger.LogInformation("Request made to update user. Response Message: {message}", message);
            return user;
        }
    }
}