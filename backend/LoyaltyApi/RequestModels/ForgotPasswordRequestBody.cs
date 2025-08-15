using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyApi.RequestModels
{
    public class ForgotPasswordRequestBody
    {
        public required string Email { get; set; }
        public required int RestaurantId { get; set; }
    }
}