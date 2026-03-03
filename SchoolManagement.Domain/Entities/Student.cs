using SchoolManagement.Domain.Entities.Authentication;
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
    [Table("Students")]
    public class Student : BaseEntity
    {
        [Required]
        [StringLength(20)]
        [Column(TypeName = "nvarchar(20)")]
        public string StudentCode { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string? LastName { get; set; }

        [StringLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string FullName { get; set; }

        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string? GuardianName { get; set; }

        [StringLength(20)]
        [Column(TypeName = "nvarchar(20)")]
        [Phone]
        public string? GuardianPhone { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        [Column(TypeName = "nvarchar(20)")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public Gender Gender { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public StudentClass Class { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime AdmissionDate { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public StudentStatus Status { get; set; }

        [Required]
        [StringLength(200)]
        [Column(TypeName = "nvarchar(200)")]
        public string Address { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string City { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public Country Country { get; set; }
    }
}