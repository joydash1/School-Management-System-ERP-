using SchoolManagement.Domain.DTOs.CommonDtos;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Interfaces
{
    public interface IUsersRepositories
    {
        Task<Result> UserRegistrationAsync();

        Task<bool> IsUserExistsAsync(string email);

        Task<bool> IsUserExistsByIdAsync(Guid userId);
    }
}