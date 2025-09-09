using NUnit.Framework;
using LoyaltyApi.Tests.E2E.Helpers;
using System.Text.Json;

namespace LoyaltyApi.Tests.E2E.Voucher;

[TestFixture]
public class VoucherTests : BaseTest
{
    private string testUserEmail = string.Empty;
    private string testUserPassword = "Test123!";
    private int restaurantId = 202;
    private int userId;
    private string accessToken = string.Empty;

    [SetUp]
    public async Task VoucherSetup()
    {

        // Register user
        var registerData = TestHelper.CreateTestUser();
        var registerResponse = await MakeApiRequest("POST", "/api/users", registerData);

        if (registerResponse.Status == 200 || registerResponse.Status == 201)
        {
            // Extract user ID correctly
            var registerResponseText = await registerResponse.TextAsync();
            var registerResult = JsonSerializer.Deserialize<JsonElement>(registerResponseText);

            userId = registerResult.GetProperty("data")
                                     .GetProperty("user")
                                     .GetProperty("id")
                                     .GetInt32();

            // Also extract access token for authenticated requests

        }
        var loginBody = TestHelper.CreateLoginRequest(registerData.Email, registerData.Password, registerData.PhoneNumber, restaurantId);
        var loginResponse = await MakeApiRequest("POST", "/api/auth/login", loginBody);
        var loginResponseText = await loginResponse.TextAsync();
        var loginResult = JsonSerializer.Deserialize<JsonElement>(loginResponseText);
        accessToken = loginResult.GetProperty("data")
                                  .GetProperty("accessToken")
                                  .GetString()!;
        testUserEmail = registerData.Email;
        testUserPassword = registerData.Password!;
        // Add points to user

    }

    [Test]
    public async Task GenerateVoucher_ValidRequest_ShouldCreateVoucher()
    {
        //Add Points to user
        var addPoints = JsonSerializer.Serialize(new
        {
            receiptId = 1,
            restaurantId = 202,
            amount = 10,
            customerId = userId,
        });

        await MakeApiRequest("POST", "/api/admin/credit-points-transactions", addPoints);
        // Arrange
        var voucherData = JsonSerializer.Serialize(new
        {
            points = 10,
        });

        // Act
        var response = await MakeAuthenticatedApiRequest("POST", "/api/vouchers", accessToken, voucherData);

        // Assert
        Assert.That(response.Status, Is.EqualTo(200).Or.EqualTo(201), "Voucher generation should succeed");
        Console.WriteLine("Voucher Generation Response Status: " + response.Status);
        var responseText = await response.TextAsync();
        Console.WriteLine("Voucher Generation Response: " + responseText);
        Assert.That(responseText, Is.Not.Empty, "Response should contain voucher data");
    }
    [Test]
    public async Task GenerateVoucher_InvalideRequest_ExpiredPoints()
    {
        var addPoints = JsonSerializer.Serialize(new
        {
            receiptId = 1,
            restaurantId = 202,
            amount = 10,
            customerId = userId,
            transactionDate = "1999-09-08T12:00:00Z"
        });

        await MakeApiRequest("POST", "/api/admin/credit-points-transactions", addPoints);
        // Arrange
        var voucherData = JsonSerializer.Serialize(new
        {
            points = 10, // Assuming user doesn't have this many points
        });

        // Act
        var response = await MakeAuthenticatedApiRequest("POST", "/api/vouchers", accessToken, voucherData);

        // Assert
        Assert.That(response.Status, Is.EqualTo(410).Or.EqualTo(400).Or.EqualTo(409), "Voucher generation should fail due to insufficient points");
        Console.WriteLine("Voucher Generation Invalid Request Response Status: " + response.Status);
        var responseText = await response.TextAsync();
        Console.WriteLine("Voucher Generation Invalid Request Response: " + responseText);
        Assert.That(responseText, Is.Not.Empty, "Response should contain error message");
    }

    [Test]
    public async Task GenerateVoucher_InvalideRequest_NotEnoughPoints()
    {
        var addPoints = JsonSerializer.Serialize(new
        {
            receiptId = 1,
            restaurantId = 202,
            amount = 10,
            customerId = userId
        });

        await MakeApiRequest("POST", "/api/admin/credit-points-transactions", addPoints);
        // Arrange
        var voucherData = JsonSerializer.Serialize(new
        {
            points = 1000, // Assuming user doesn't have this many points
        });

        // Act
        var response = await MakeAuthenticatedApiRequest("POST", "/api/vouchers", accessToken, voucherData);

        // Assert
        Assert.That(response.Status, Is.EqualTo(410).Or.EqualTo(400).Or.EqualTo(409), "Voucher generation should fail due to insufficient points");
        Console.WriteLine("Voucher Generation Invalid Request Response Status: " + response.Status);
        var responseText = await response.TextAsync();
        Console.WriteLine("Voucher Generation Invalid Request Response: " + responseText);
        Assert.That(responseText, Is.Not.Empty, "Response should contain error message");
    }
}
