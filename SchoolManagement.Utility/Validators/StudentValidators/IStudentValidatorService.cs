using FluentValidation.Results;
using SchoolManagement.Domain.DTOs.StudentsDTOS;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Utility.Validators.StudentValidators
{
    public interface IStudentValidatorService
    {
        Task<ValidationResult> ValidateAsync(SaveStudentDTO entity, CancellationToken cancellationToken = default);

        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);

        Task<bool> IsStudentCodeUniqueAsync(string studentCode, int? excludeId = null, CancellationToken cancellationToken = default);
    }
}