using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.DataAccess.DataContext;
using SchoolManagement.Domain.Entities.Authentication;
using SchoolManagement.Domain.Interfaces.AuthenticationAndAuthorization;
using SchoolManagement.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DataAccess.Repositories
{
    public class RoleRepository : GenericRepository<IdentityRole>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IdentityRole?> GetByNameAsync(string roleName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task<IEnumerable<IdentityRole>> GetAllWithUserCountAsync()
        {
            var roles = await _dbSet.ToListAsync();
            return roles;
        }

        public async Task<int> GetUserCountInRoleAsync(string roleName)
        {
            var role = await GetByNameAsync(roleName);
            if (role == null)
                return 0;

            return await _dbContext.UserRoles
                .CountAsync(ur => ur.RoleId == role.Id);
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _dbSet
                .AnyAsync(r => r.Name == roleName);
        }

        public async Task<IEnumerable<ApplicationUsers>> GetUsersInRoleAsync(string roleName)
        {
            var role = await GetByNameAsync(roleName);
            if (role == null)
                return new List<ApplicationUsers>();

            var userIds = await _dbContext.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _dbContext.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }
    }
}