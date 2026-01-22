using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Interfaces.CommonRepositories
{
    public interface IJwtTokenRepositories
    {
        string GenerateToken(Users users);
    }
}