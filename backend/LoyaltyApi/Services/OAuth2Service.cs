using LoyaltyApi.Config;
using Microsoft.Extensions.Options;
using LoyaltyApi.Repositories;
using LoyaltyApi.Models;
using System.Text.Json;

namespace LoyaltyApi.Services
{
    public class OAuth2Service(HttpClient httpClient)
    {
        public async Task<User> HandleGoogleSignIn(string accessToken)
        {
            string url = "https://www.googleapis.com/oauth2/v3/userinfo?access_token=" + accessToken;
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) throw new HttpRequestException("Failed to get user info from Google.");
            var json = await response.Content.ReadAsStringAsync();
            var userJson = JsonSerializer.Deserialize<JsonElement>(json);
            return new User
            {
                Email = userJson.GetProperty("email").GetString() ?? throw new ArgumentException("Email is missing"),
                Name = userJson.GetProperty("name").GetString() ?? throw new ArgumentException("Name is missing"),
            };
        }
        public async Task<User> HandleFacebookSignIn(string accessToken)
        {
            string url = "https://graph.facebook.com/me?fields=name,email&access_token=" + accessToken;
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) throw new HttpRequestException("Failed to get user info from Facebook.");
            var json = await response.Content.ReadAsStringAsync();
            var userJson = JsonSerializer.Deserialize<JsonElement>(json);
            return new User
            {
                Email = userJson.GetProperty("email").GetString() ?? throw new ArgumentException("Email is missing"),
                Name = userJson.GetProperty("name").GetString() ?? throw new ArgumentException("Name is missing"),
            };
        }
    }
}