using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.DataAccess.DataContext;
using SchoolManagement.DataAccess.Repositories.Auth;
using SchoolManagement.DataAccess.UnitOfWork;
using SchoolManagement.Domain.DTOs.StudentsDTOS;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DataAccess.Repositories
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        private readonly ApplicationDbContext _studentContext;
        private readonly ILogger<StudentRepository> _logger;

        public StudentRepository(ApplicationDbContext studentContext, ILoggerFactory loggerFactory) : base(studentContext, loggerFactory)
        {
            _studentContext = studentContext;
            _logger = loggerFactory?.CreateLogger<StudentRepository>();
        }

        public async Task<Student> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.Email == email, cancellationToken);
        }

        public async Task<Student> GetByStudentCodeAsync(string studentCode, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.StudentCode == studentCode, cancellationToken);
        }

        public async Task<IEnumerable<Student>> GetActiveStudentsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.IsActive)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Student>> GetStudentsByAgeRangeAsync(
            int minAge,
            int maxAge,
            CancellationToken cancellationToken = default)
        {
            var minDate = DateTime.Today.AddYears(-maxAge);
            var maxDate = DateTime.Today.AddYears(-minAge);

            return await _dbSet
                .Where(s => s.DateOfBirth >= minDate && s.DateOfBirth <= maxDate)
                .OrderBy(s => s.DateOfBirth)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(s => s.Email == email);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> IsStudentCodeUniqueAsync(string studentCode, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(s => s.StudentCode == studentCode);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Student> Students, int TotalCount)> SearchStudentsAsync(
            string searchTerm,
            int pageIndex = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(s =>
                    s.FirstName.ToLower().Contains(searchTerm) ||
                    s.LastName.ToLower().Contains(searchTerm) ||
                    s.Email.ToLower().Contains(searchTerm) ||
                    s.StudentCode.ToLower().Contains(searchTerm) ||
                    s.PhoneNumber.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var students = await query
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (students, totalCount);
        }
    }
}