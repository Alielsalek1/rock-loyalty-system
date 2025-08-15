using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyApi.RequestModels
{
    public class UpdatePasswordRequestModel
    {
        public required string Password { get; set; }
    }
}