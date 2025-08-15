using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyApi.RequestModels
{
    public class OAuth2Body
    {
        public required string AccessToken { get; set; }

        public required int RestaurantId { get; set; }
    }
}