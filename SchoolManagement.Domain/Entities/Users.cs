using Microsoft.AspNetCore.Http;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Entities
{
    public class Users
    {
        public Guid Id { get; private set; }

        public string FullName { get; private set; }
        public string Email { get; private set; }
        public string MobileNo { get; private set; }
        public string EmployeeCode { get; private set; }

        public string PasswordHash { get; private set; }

        public UserRole Role { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }

        protected Users()
        { }

        public Users(
        string fullName,
        string email,
        string mobileNo,
        string employeeCode,
        string passwordHash,
        UserRole role)
        {
            Id = Guid.NewGuid();
            FullName = fullName;
            Email = email;
            MobileNo = mobileNo;
            EmployeeCode = employeeCode;
            PasswordHash = passwordHash;
            Role = role;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateLastLogin()
            => LastLoginAt = DateTime.UtcNow;
    }
}