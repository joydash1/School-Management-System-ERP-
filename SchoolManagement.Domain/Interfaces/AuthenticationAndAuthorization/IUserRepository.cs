using SchoolManagement.Domain.DTOs.AuthenticationAndAuthorizationDtos;
using SchoolManagement.Domain.Entities.Authentication;
using SchoolManagement.Domain.Interfaces.CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Interfaces.AuthenticationAndAuthorization
{
    public interface IUserRepository : IGenericRepository<ApplicationUsers>
    {
        // User specific methods
        Task<ApplicationUsers?> GetByEmailAsync(string email);

        Task<ApplicationUsers?> GetByUserNameAsync(string userName);

        Task<IEnumerable<ApplicationUsers>> GetUsersByRoleAsync(string roleName);

        Task<IEnumerable<ApplicationUsers>> GetActiveUsersAsync();

        Task<IEnumerable<ApplicationUsers>> GetInactiveUsersAsync();

        Task<bool> IsEmailConfirmedAsync(string userId);

        Task<IList<string>> GetUserRolesAsync(string userId);

        Task<UserDto?> GetUserWithRolesAsync(string userId);

        Task<IEnumerable<UserDto>> GetAllUsersWithRolesAsync();

        Task<bool> UpdateLastLoginAsync(string userId);
    }
}