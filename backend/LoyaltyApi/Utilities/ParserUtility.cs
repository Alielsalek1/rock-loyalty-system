using System.Text.RegularExpressions;

namespace LoyaltyApi.Utilities
{
    public class ParserUtility(ILogger<ParserUtility> logger)
    {
        /// <summary>
        /// Parses a user response string and extracts customer ID from the response
        /// </summary>
        /// <param name="response">The response string to parse</param>
        /// <returns>JSON object containing either customer ID or the original response</returns>
        public object UserParser(string response)
        {
            // Check if response contains "Customer Number" followed by digits
            var customerNumberPattern = @"Customer\s+Number\s+(\d+)";
            var match = Regex.Match(response, customerNumberPattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                var customerNumber = int.Parse(match.Groups[1].Value);
                return new
                {
                    success = true,
                    customerId = customerNumber,
                    response = "Customer created successfully"
                };
            }

            return new
            {
                success = false,
                response = "User not created",
                originalResponse = response
            };
        }
        
    }
}