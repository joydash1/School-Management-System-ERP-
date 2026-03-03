using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Interfaces.CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Interfaces
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        Task<Student> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<Student> GetByStudentCodeAsync(string studentCode, CancellationToken cancellationToken = default);

        Task<IEnumerable<Student>> GetActiveStudentsAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<Student>> GetStudentsByAgeRangeAsync(int minAge, int maxAge, CancellationToken cancellationToken = default);

        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);

        Task<bool> IsStudentCodeUniqueAsync(string studentCode, int? excludeId = null, CancellationToken cancellationToken = default);

        Task<(IEnumerable<Student> Students, int TotalCount)> SearchStudentsAsync(
            string searchTerm,
            int pageIndex = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
    }
}