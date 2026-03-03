using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.DataAccess.Repositories;
using SchoolManagement.DataAccess.UnitOfWork;
using SchoolManagement.Domain.DTOs.CommonDtos;
using SchoolManagement.Domain.DTOs.StudentsDTOS;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Utility.Validators.StudentValidators;
using System.Threading;

namespace SchoolManagement.WEB.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudentValidatorService _studentValidatorService;
        private readonly ILogger<StudentRepository> _logger;

        public StudentsController(IUnitOfWork unitOfWork, IStudentValidatorService studentValidatorService, ILogger<StudentRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _studentValidatorService = studentValidatorService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("SaveUpdateStudent")]
        public async Task<ActionResult<Result>> SaveUpdateStudentAsync([FromForm] SaveStudentDTO dto, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var validationResult = await _studentValidatorService.ValidateAsync(dto);

                if (!validationResult.IsValid)
                {
                    await _unitOfWork.RollbackAsync();
                    var errorMessages = validationResult.Errors
                                       .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
                                       .ToList();

                    return BadRequest(Result.Failure(
                        errors: errorMessages,
                        statusCode: StatusCodes.Status400BadRequest
                    ));
                }
                // Map DTOs To Entity
                var student = new Student
                {
                    Id = dto.Id,
                    StudentCode = dto.StudentCode,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    FullName = dto.FullName,
                    GuardianName = dto.GuardianName,
                    GuardianPhone = dto.GuardianPhone,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    Class = dto.Class,
                    AdmissionDate = dto.AdmissionDate,
                    Status = dto.Status,
                    Address = dto.Address,
                    City = dto.City,
                    Country = dto.Country
                };

                if (student.Id == 0)
                {
                    student.CreatedAt = DateTime.UtcNow;
                    student.CreatedBy = 1;
                    student.IsActive = true;
                    await _unitOfWork.StudentRepository.AddAsync(student, cancellationToken);
                }
                else
                {
                    // Update - check if exists
                    var existingStudent = await _unitOfWork.StudentRepository
                        .FindSingleAsync(x => x.Id == student.Id, cancellationToken);

                    if (existingStudent == null)
                        throw new KeyNotFoundException($"Student with ID {student.Id} not found");

                    student.CreatedAt = existingStudent.CreatedAt;
                    student.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.StudentRepository.Update(student);
                }

                await _unitOfWork.CommitAsync();

                var message = "";
                if (dto.Id == 0)
                {
                    message = $"Student Name:{student.FirstName}, Code {student.StudentCode} Save Successfully.";
                }
                else
                {
                    message = $"New Student Name:{student.FirstName}, Code {student.StudentCode} Update Successfully.";
                }

                return Ok(Result.Success(message));
            }
            catch (KeyNotFoundException ex)
            {
                await _unitOfWork.RollbackAsync();
                return NotFound(Result.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error saving student");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    Result.Failure(
                        error: "An unexpected error occurred while saving the student",
                        statusCode: StatusCodes.Status500InternalServerError
                    )
                );
            }
        }
    }
}