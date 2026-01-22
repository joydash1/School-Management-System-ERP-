using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.DTOs.AuthenticationDtos
{
    public sealed record RegisterRequestDto(
         string FullName,
         string Email,
         string MobileNo,
         string EmployeeCode,
         string PasswordHash,
         UserRole Role
    );
}