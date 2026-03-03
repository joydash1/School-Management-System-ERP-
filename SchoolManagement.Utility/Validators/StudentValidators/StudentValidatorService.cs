using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.DataAccess.DataContext;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Utility.Validators.StudentValidators
{
    using FluentValidation.Results;
    using Microsoft.EntityFrameworkCore;
    using SchoolManagement.Domain.DTOs.StudentsDTOS;

    public class StudentValidatorService : IStudentValidatorService
    {
        private readonly ApplicationDbContext _context;

        public StudentValidatorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ValidationResult> ValidateAsync(SaveStudentDTO entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var errors = new List<ValidationFailure>();

            if (string.IsNullOrWhiteSpace(entity.FirstName))
            {
                errors.Add(new ValidationFailure("FirstName", "First name is required"));
            }
            else
            {
                if (entity.FirstName.Length > 50)
                {
                    errors.Add(new ValidationFailure("FirstName", "First name cannot exceed 50 characters"));
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(entity.FirstName, @"^[a-zA-Z\s-']+$"))
                {
                    errors.Add(new ValidationFailure("FirstName", "First name can only contain letters, spaces, hyphens, and apostrophes"));
                }
            }

            if (string.IsNullOrWhiteSpace(entity.LastName))
            {
                errors.Add(new ValidationFailure("LastName", "Last name is required"));
            }
            else
            {
                if (entity.LastName.Length > 50)
                {
                    errors.Add(new ValidationFailure("LastName", "Last name cannot exceed 50 characters"));
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(entity.LastName, @"^[a-zA-Z\s-']+$"))
                {
                    errors.Add(new ValidationFailure("LastName", "Last name can only contain letters, spaces, hyphens, and apostrophes"));
                }
            }
            if (string.IsNullOrWhiteSpace(entity.FullName))
            {
                errors.Add(new ValidationFailure("FullName", "Full name is required"));
            }
            else
            {
                if (entity.FullName.Length > 50)
                {
                    errors.Add(new ValidationFailure("FullName", "Full name cannot exceed 50 characters"));
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(entity.FullName, @"^[a-zA-Z\s-']+$"))
                {
                    errors.Add(new ValidationFailure("FullName", "Full name can only contain letters, spaces, hyphens, and apostrophes"));
                }
            }

            if (string.IsNullOrWhiteSpace(entity.Email))
            {
                errors.Add(new ValidationFailure("Email", "Email is required"));
            }
            else
            {
                if (entity.Email.Length > 100)
                {
                    errors.Add(new ValidationFailure("Email", "Email cannot exceed 100 characters"));
                }

                if (!IsValidEmail(entity.Email))
                {
                    errors.Add(new ValidationFailure("Email", "Please enter a valid email address"));
                }
            }

            if (string.IsNullOrWhiteSpace(entity.PhoneNumber))
            {
                errors.Add(new ValidationFailure("PhoneNumber", "Phone number is required"));
            }
            else
            {
                if (entity.PhoneNumber.Length > 20)
                {
                    errors.Add(new ValidationFailure("PhoneNumber", "Phone number cannot exceed 20 characters"));
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(entity.PhoneNumber, @"^\+?[1-9]\d{1,14}$"))
                {
                    errors.Add(new ValidationFailure("PhoneNumber", "Please enter a valid international phone number"));
                }
            }

            if (string.IsNullOrWhiteSpace(entity.StudentCode))
            {
                errors.Add(new ValidationFailure("StudentCode", "Student code is required"));
            }
            else
            {
                if (entity.StudentCode.Length > 20)
                {
                    errors.Add(new ValidationFailure("StudentCode", "Student code cannot exceed 20 characters"));
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(entity.StudentCode, @"^[A-Z0-9-]+$"))
                {
                    errors.Add(new ValidationFailure("StudentCode", "Student code can only contain uppercase letters, numbers, and hyphens"));
                }
            }
            if (string.IsNullOrWhiteSpace(entity.Address))
            {
                errors.Add(new ValidationFailure("Address", "Address is required"));
            }
            else if (entity.Address.Length > 200)
            {
                errors.Add(new ValidationFailure("Address", "Address cannot exceed 200 characters"));
            }
            if (string.IsNullOrWhiteSpace(entity.City))
            {
                errors.Add(new ValidationFailure("City", "City is required"));
            }
            else if (entity.City.Length > 50)
            {
                errors.Add(new ValidationFailure("City", "City cannot exceed 50 characters"));
            }

            if (entity.DateOfBirth == default || entity.DateOfBirth == DateTime.MinValue)
            {
                errors.Add(new ValidationFailure("DateOfBirth", "Date of birth is required"));
            }
            else
            {
                if (entity.DateOfBirth > DateTime.Today)
                {
                    errors.Add(new ValidationFailure("DateOfBirth", "Date of birth cannot be in the future"));
                }

                if (entity.DateOfBirth < DateTime.Today.AddYears(-100))
                {
                    errors.Add(new ValidationFailure("DateOfBirth", "Please enter a valid date of birth"));
                }
            }

            if (entity.AdmissionDate == default || entity.AdmissionDate == DateTime.MinValue)
            {
                errors.Add(new ValidationFailure("AdmissionDate", "Admission date is required"));
            }
            else
            {
                if (entity.AdmissionDate > DateTime.Today)
                {
                    errors.Add(new ValidationFailure("AdmissionDate", "Admission date cannot be in the future"));
                }

                if (entity.AdmissionDate < entity.DateOfBirth.AddYears(5))
                {
                    errors.Add(new ValidationFailure("AdmissionDate", "Student must be at least 5 years old to admit"));
                }
            }

            if (entity.Gender == 0)
            {
                errors.Add(new ValidationFailure("Gender", "Please select a gender"));
            }

            if (entity.Class == 0)
            {
                errors.Add(new ValidationFailure("Class", "Please select a class"));
            }

            if (entity.Status == 0)
            {
                errors.Add(new ValidationFailure("Status", "Please select a status"));
            }

            if (entity.Country == 0)
            {
                errors.Add(new ValidationFailure("Country", "Please select a country"));
            }

            if (!errors.Any(e => e.PropertyName == "Email"))
            {
                if (!await IsEmailUniqueAsync(entity.Email, entity.Id, cancellationToken))
                {
                    errors.Add(new ValidationFailure("Email", "Email already exists in the system"));
                }
            }

            if (!errors.Any(e => e.PropertyName == "StudentCode"))
            {
                if (!await IsStudentCodeUniqueAsync(entity.StudentCode, entity.Id, cancellationToken))
                {
                    errors.Add(new ValidationFailure("StudentCode", "Student code already exists in the system"));
                }
            }

            var age = CalculateAge(entity.DateOfBirth);

            if (age < 18)
            {
                if (string.IsNullOrWhiteSpace(entity.GuardianName))
                {
                    errors.Add(new ValidationFailure("GuardianName",
                        "Guardian name is required for students under 18 years of age"));
                }
                else if (entity.GuardianName.Length > 100)
                {
                    errors.Add(new ValidationFailure("GuardianName",
                        "Guardian name cannot exceed 100 characters"));
                }

                if (string.IsNullOrWhiteSpace(entity.GuardianPhone))
                {
                    errors.Add(new ValidationFailure("GuardianPhone",
                        "Guardian phone is required for students under 18 years of age"));
                }
                else if (entity.GuardianPhone.Length > 20)
                {
                    errors.Add(new ValidationFailure("GuardianPhone",
                        "Guardian phone cannot exceed 20 characters"));
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(entity.GuardianPhone, @"^\+?[1-9]\d{1,14}$"))
                {
                    errors.Add(new ValidationFailure("GuardianPhone",
                        "Please enter a valid international phone number for guardian"));
                }
            }

            if (age >= 18 && !string.IsNullOrEmpty(entity.GuardianName) && string.IsNullOrEmpty(entity.GuardianPhone))
            {
                errors.Add(new ValidationFailure("GuardianPhone",
                    "Guardian phone number is required when guardian name is provided"));
            }

            if (!string.IsNullOrEmpty(entity.GuardianPhone))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(entity.GuardianPhone, @"^\+?[1-9]\d{1,14}$"))
                {
                    errors.Add(new ValidationFailure("GuardianPhone",
                        "Please enter a valid international phone number for guardian"));
                }
            }

            return new ValidationResult(errors);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(email))
                return true;

            var query = _context.Students.Where(x => x.Email == email);

            if (excludeId.HasValue && excludeId.Value > 0)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> IsStudentCodeUniqueAsync(string studentCode, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(studentCode))
                return true;

            var query = _context.Students.Where(x => x.StudentCode == studentCode);

            if (excludeId.HasValue && excludeId.Value > 0)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }

        private static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}