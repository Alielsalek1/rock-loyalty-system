
using LoyaltyApi.Models;

namespace LoyaltyApi.Repositories
{
    public interface IPasswordRepository
    {
        Task<Password?> GetPasswordAsync(Password password);
        Task<Password> UpdatePasswordAsync(Password password);
        Task<Password> CreatePasswordAsync(Password password);
    }
}