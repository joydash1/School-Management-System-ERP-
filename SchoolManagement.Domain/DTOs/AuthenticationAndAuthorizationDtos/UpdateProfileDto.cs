using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.DTOs.AuthenticationAndAuthorizationDtos
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "First Name is Required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Profile picture is required")]
        [DataType(DataType.Upload)]
        public IFormFile? ProfilePicture { get; set; }
    }
}