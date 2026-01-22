using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.DTOs.AuthenticationDtos
{
    public sealed record UserProfileDto(
        int Id,
        string FullName,
        string Email,
        string Role,
        string? ProfilePicturePath,
        string JobTitle,
        string EmployeeCode,
        string MobileNo,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? LastLoginAt
    );
}