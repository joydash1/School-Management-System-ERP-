using Microsoft.EntityFrameworkCore;
using SchoolManagement.DataAccess.DataContext;
using SchoolManagement.Domain.Entities.AuthenticationAndAuthorization;
using SchoolManagement.Domain.Interfaces.AuthenticationAndAuthorization;
using SchoolManagement.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DataAccess.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(string userId)
        {
            return await _dbSet
                .Where(rt => rt.UserId == userId
                    && !rt.IsUsed
                    && !rt.IsRevoked
                    && rt.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task RevokeAllUserTokensAsync(string userId)
        {
            var tokens = await _dbSet
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            _dbSet.UpdateRange(tokens);
        }

        public async Task<RefreshToken?> GetValidTokenAsync(string token, string userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(rt => rt.Token == token
                    && rt.UserId == userId
                    && !rt.IsUsed
                    && !rt.IsRevoked
                    && rt.ExpiryDate > DateTime.UtcNow);
        }

        public async Task RemoveExpiredTokensAsync()
        {
            var expiredTokens = await _dbSet
                .Where(rt => rt.ExpiryDate <= DateTime.UtcNow)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                _dbSet.RemoveRange(expiredTokens);
            }
        }
    }
}