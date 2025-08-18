using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyApi.Exceptions
{
    public class ApiUtilityException(string? message) : Exception(message)
    {

    }
}