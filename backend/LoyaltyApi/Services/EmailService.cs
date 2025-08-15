using System.Threading.Tasks;
using LoyaltyApi.Config;
using Microsoft.Extensions.Logging;
using FluentEmail.Core;

namespace LoyaltyApi.Services
{
    public class EmailService(IFluentEmail fluentEmail,
    ILogger<EmailService> logger)
    {
        public async Task SendEmailAsync(string email, string subject, string message, string name)
        {
            var result = await fluentEmail
                .To(email)
                .Subject(subject)
                .Body(message, isHtml: true)
                .SendAsync();

            logger.LogInformation("Email sent to {email}. Success: {success}", email, result.Successful);

            if (!result.Successful)
            {
                logger.LogError("Failed to send email: {errors}", string.Join(", ", result.ErrorMessages));
                throw new InvalidOperationException($"Failed to send email: {string.Join(", ", result.ErrorMessages)}");
            }
        }
    }
}