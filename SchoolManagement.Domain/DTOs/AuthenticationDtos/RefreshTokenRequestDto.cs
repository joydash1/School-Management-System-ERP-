using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.DTOs.AuthenticationDtos
{
    public sealed record RefreshTokenRequestDto(
        [Required(ErrorMessage = "Access token is required")]
        string AccessToken,

        [Required(ErrorMessage = "Refresh token is required")]
        string RefreshToken
    );
}