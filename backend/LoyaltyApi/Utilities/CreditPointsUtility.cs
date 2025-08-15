using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyApi.Utilities
{
    public class CreditPointsUtility
    {
        public int CalculateCreditPoints(double amount, double ratio) => Convert.ToInt32(amount * (ratio / 100));
    }
}