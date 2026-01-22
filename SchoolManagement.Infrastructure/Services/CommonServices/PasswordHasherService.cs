using Microsoft.AspNetCore.Identity;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Interfaces.CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Services.CommonServices
{
    public class PasswordHasherService : IPasswordHasherRepository
    {
        private readonly PasswordHasher<Users> _hasher = new();

        public string Hash(string password)
        => _hasher.HashPassword(null!, password);

        public bool Verify(string password, string hash)
        => _hasher.VerifyHashedPassword(null!, hash, password) == PasswordVerificationResult.Success;
    }
}