using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyApi.Exceptions
{
    public class MinimumPointsNotReachedException(string? message) : Exception(message)
    {

    }
}