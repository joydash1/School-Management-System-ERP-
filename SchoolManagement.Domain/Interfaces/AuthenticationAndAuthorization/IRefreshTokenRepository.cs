using SchoolManagement.Domain.Entities.AuthenticationAndAuthorization;
using SchoolManagement.Domain.Interfaces.CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Interfaces.AuthenticationAndAuthorization
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);

        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(string userId);

        Task RevokeAllUserTokensAsync(string userId);

        Task<RefreshToken?> GetValidTokenAsync(string token, string userId);

        Task RemoveExpiredTokensAsync();
    }
}