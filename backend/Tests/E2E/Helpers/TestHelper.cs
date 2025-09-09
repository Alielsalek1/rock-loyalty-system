using System.Text.Json;

namespace LoyaltyApi.Tests.E2E.Helpers;

public static class TestHelper
{
    public static readonly Random Random = new();
    
    public static string GenerateRandomEmail()
    {
        return $"test{Random.Next(1000, 9999)}@example.com";
    }
    
    public static string GenerateRandomPhoneNumber()
    {
        return $"+201{Random.Next(100000000, 999999999)}";
    }
    
    public static string GenerateRandomString(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
    
    public static async Task<T?> ParseJsonResponse<T>(Microsoft.Playwright.IAPIResponse response)
    {
        var jsonString = await response.TextAsync();
        return JsonSerializer.Deserialize<T>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    
    public static TestUserData CreateTestUser()
    {
        return new TestUserData
        {
            Name = "Test User",
            Email = GenerateRandomEmail(),
            PhoneNumber = GenerateRandomPhoneNumber(),
            Password = "Test123!",
            RestaurantId = 202
        };
    }

    public static string SerializeTestUser(TestUserData user)
    {
        return JsonSerializer.Serialize(new
        {
            name = user.Name,
            email = user.Email,
            phoneNumber = user.PhoneNumber,
            password = user.Password,
            restaurantId = user.RestaurantId
        });
    }

    public class TestUserData
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
    }

    public static string CreateLoginRequest(string email, string password, string phoneNumber = null, int restaurantId = 202)
    {
        return JsonSerializer.Serialize(new
        {
            email = email,
            phoneNumber = phoneNumber ?? GenerateRandomPhoneNumber(),
            password = password,
            restaurantId = restaurantId
        });
    
    }
}
