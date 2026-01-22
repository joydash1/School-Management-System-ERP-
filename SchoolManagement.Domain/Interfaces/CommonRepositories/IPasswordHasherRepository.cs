using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Interfaces.CommonRepositories
{
    public interface IPasswordHasherRepository
    {
        string Hash(string password);

        bool Verify(string password, string hash);
    }
}