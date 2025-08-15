using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyApi.RequestModels
{
    public class SetIsUsedRequestModel
    {
        public bool IsUsed { get; set; }
        public int RestaurantId { get; set; }
        public int CustomerId { get; set; }
    }
}