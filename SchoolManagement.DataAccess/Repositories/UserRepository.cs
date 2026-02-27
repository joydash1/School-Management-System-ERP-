using Microsoft.EntityFrameworkCore;
using SchoolManagement.DataAccess.DataContext;
using SchoolManagement.Domain.DTOs.AuthenticationAndAuthorizationDtos;
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
    public class UserRepository : GenericRepository<ApplicationUsers>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ApplicationUsers?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUsers?> GetByUserNameAsync(string userName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<IEnumerable<ApplicationUsers>> GetUsersByRoleAsync(string roleName)
        {
            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
                return new List<ApplicationUsers>();

            var userIds = await _dbContext.UserRoles
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _dbSet
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUsers>> GetActiveUsersAsync()
        {
            return await _dbSet
                .Where(u => u.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUsers>> GetInactiveUsersAsync()
        {
            return await _dbSet
                .Where(u => !u.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsEmailConfirmedAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            return user?.EmailConfirmed ?? false;
        }

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return new List<string>();

            var roles = await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_dbContext.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name)
                .ToListAsync();

            return roles!;
        }

        public async Task<UserDto?> GetUserWithRolesAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return null;

            var roles = await GetUserRolesAsync(userId);

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersWithRolesAsync()
        {
            var users = await _dbSet.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await GetUserRolesAsync(user.Id);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    IsActive = user.IsActive
                });
            }

            return userDtos;
        }

        public async Task<bool> UpdateLastLoginAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return false;

            user.LastLoginAt = DateTime.UtcNow;
            Update(user);
            return true;
        }
    }
}