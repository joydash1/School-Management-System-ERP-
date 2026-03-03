using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Domain.DTOs.StudentsDTOS
{
    //public sealed record SaveStudentDTO(
    //    int Id,
    //    string StudentCode,
    //    string FirstName,
    //    string? LastName,
    //    string? GuardianName,
    //    string? GuardianPhone,
    //    string Email,
    //    string PhoneNumber,
    //    DateTime DateOfBirth,
    //    Gender Gender,
    //    StudentClass Class,
    //    DateTime AdmissionDate,
    //    StudentStatus Status,
    //    string Address,
    //    string City,
    //    Country Country
    //);

    public class SaveStudentDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Student code is required")]
        [StringLength(20, ErrorMessage = "Student code cannot exceed 20 characters")]
        [RegularExpression(@"^[A-Z0-9-]+$", ErrorMessage = "Student code can only contain uppercase letters, numbers, and hyphens")]
        public string StudentCode { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string? LastName { get; set; }

        [StringLength(50, ErrorMessage = "Full Name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s-']+$", ErrorMessage = "Full Name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FullName { get; set; }

        [StringLength(100, ErrorMessage = "Guardian name cannot exceed 100 characters")]
        public string? GuardianName { get; set; }

        [StringLength(20, ErrorMessage = "Guardian phone cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string? GuardianPhone { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Class is required")]
        public StudentClass Class { get; set; }

        [Required(ErrorMessage = "Admission date is required")]
        [DataType(DataType.Date)]
        public DateTime AdmissionDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public StudentStatus Status { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string City { get; set; }

        [Required(ErrorMessage = "Country is required")]
        public Country Country { get; set; }
    }
}