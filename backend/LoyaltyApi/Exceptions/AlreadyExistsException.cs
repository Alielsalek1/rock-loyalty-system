using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoyaltyApi.Exceptions
{
    public class AlreadyExistsException(string? message) : Exception(message)
    {

    }
}