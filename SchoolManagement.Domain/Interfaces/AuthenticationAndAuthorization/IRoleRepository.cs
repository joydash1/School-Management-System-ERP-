using Microsoft.AspNetCore.Identity;
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
    public interface IRoleRepository : IGenericRepository<IdentityRole>
    {
        Task<IdentityRole?> GetByNameAsync(string roleName);

        Task<IEnumerable<IdentityRole>> GetAllWithUserCountAsync();

        Task<int> GetUserCountInRoleAsync(string roleName);

        Task<bool> RoleExistsAsync(string roleName);

        Task<IEnumerable<ApplicationUsers>> GetUsersInRoleAsync(string roleName);
    }
}