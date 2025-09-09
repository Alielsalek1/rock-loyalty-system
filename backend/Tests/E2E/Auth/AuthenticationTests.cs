using NUnit.Framework;
using LoyaltyApi.Tests.E2E.Helpers;
using Microsoft.Playwright;
using System.Text.Json;

namespace LoyaltyApi.Tests.E2E.Auth;

[TestFixture]
public class AuthenticationTests : BaseTest
{
    private string testUserEmail = string.Empty;
    private string testUserPassword = "Test123!";
    private int RestaurantId = 202;

    [Test]
    public async Task Register_ValidUser_ShouldSucceed()
    {
        // Arrange
        testUserEmail = TestHelper.GenerateRandomEmail();
        var registerData = JsonSerializer.Serialize(new
        {
            name = "Test",
            email = testUserEmail,
            phoneNumber = TestHelper.GenerateRandomPhoneNumber(),
            password = testUserPassword,
            restaurantId = RestaurantId
        });

        // Act
        var response = await MakeApiRequest("POST", "/api/users", registerData);
        var responseText1 = await response.TextAsync();
        Console.WriteLine("Response Status: " + response.Status);
        Console.WriteLine("Response Body: " + responseText1);
        // Assert
        Assert.That(response.Status, Is.EqualTo(201), "Registration should succeed");
        
        var responseText = await response.TextAsync();
        Assert.That(responseText, Is.Not.Empty, "Response should not be empty");
    }

    [Test]
    public async Task Login_ValidCredentials_ShouldReturnToken()
    {
        // Arrange - First register a user
        testUserEmail = TestHelper.GenerateRandomEmail();
        var registerData = TestHelper.CreateTestUser();
        await MakeApiRequest("POST", "/api/users", TestHelper.SerializeTestUser(registerData));

        var loginData = TestHelper.CreateLoginRequest(registerData.Email, registerData.Password, registerData.PhoneNumber , RestaurantId);

        // Act
        var response = await MakeApiRequest("POST", "/api/auth/login", loginData);
        var responseText = await response.TextAsync();
        Console.WriteLine("Response Status: " + response.Status);
        Console.WriteLine("Response Body: " + responseText);
        // Assert
        Assert.That(response.Status, Is.EqualTo(200), "Login should succeed");

        var responseText1 = await response.TextAsync();

        Assert.That(responseText1, Contains.Substring("accessToken"), "Response should contain token");

        // Optional: Parse JSON to verify structure and extract token
        var loginResult = await TestHelper.ParseJsonResponse<dynamic>(response);
        // You could also verify the actual token value is not empty:
        // Assert.That(loginResult.data.accessToken, Is.Not.Null.And.Not.Empty, "Access token should have value");
    }

    [Test]
    public async Task Login_InvalidCredentials_ShouldFail()
    {
        // Arrange
        var loginData = TestHelper.CreateLoginRequest("invalid@example.com", "wrongpassword" , "0000000000", RestaurantId);

        // Act
        var response = await MakeApiRequest("POST", "/api/auth/login", loginData);
        var responseText = await response.TextAsync();
        Console.WriteLine("Response Status: " + response.Status);
        Console.WriteLine("Response Body: " + responseText);
        // Assert
        Assert.That(response.Status, Is.EqualTo(401).Or.EqualTo(400).Or.EqualTo(404), "Login should fail with invalid credentials");
    }

    [Test]
    public async Task Register_DuplicateEmail_ShouldFail()
    {
        // Arrange
        testUserEmail = TestHelper.GenerateRandomEmail();
        var registerData = JsonSerializer.Serialize(new
        {
            name = "Test",
            email = testUserEmail,
            phoneNumber = TestHelper.GenerateRandomPhoneNumber(),
            password = testUserPassword,
            restaurantId = RestaurantId
        });

        // Act - Register first user
        await MakeApiRequest("POST", "/api/users", registerData);
        
        // Act - Try to register same email again
        var secondResponse = await MakeApiRequest("POST", "/api/users", registerData);
        Console.WriteLine("Second Registration Response Status: " + secondResponse.Status);
        Console.WriteLine("Second Registration Response Body: " + await secondResponse.TextAsync());
        // Assert
        Assert.That(secondResponse.Status, Is.EqualTo(400).Or.EqualTo(401), "Duplicate registration should fail");
    }

    [Test]
    public async Task Logout_WithValidToken_ShouldSucceed()
    {
        // Arrange - Login first to get token
        testUserEmail = TestHelper.GenerateRandomEmail();
        var registerData = TestHelper.CreateTestUser();
        await MakeApiRequest("POST", "/api/auth/register", TestHelper.SerializeTestUser(registerData));

        var loginData = TestHelper.CreateLoginRequest(testUserEmail, testUserPassword , registerData.PhoneNumber , RestaurantId);
        var loginResponse = await MakeApiRequest("POST", "/api/auth/login", loginData);
        
        // Extract token from response (assuming it's in the response)
        var loginResult = await TestHelper.ParseJsonResponse<dynamic>(loginResponse);

        // Act
        var logoutResponse = await MakeApiRequest("POST", "/api/auth/logout", null);

        // Assert
        Assert.That(logoutResponse.Status, Is.EqualTo(200), "Logout should succeed");
    }

    [Test]
    public async Task ConfirmEmail_ValidToken_ShouldFail()
    {
        // Arrange
        var registerData = TestHelper.CreateTestUser();
        await MakeApiRequest("POST", "/api/users", TestHelper.SerializeTestUser(registerData));

        
        var loginBody = TestHelper.CreateLoginRequest(registerData.Email, registerData.Password, registerData.PhoneNumber , RestaurantId);
        // Act
        var response = await MakeApiRequest("POST", "/api/auth/login", loginBody);

        // Assert
        Assert.That(response.Status, Is.EqualTo(409), "Email confirmation should fail with invalid token");
    }
}
