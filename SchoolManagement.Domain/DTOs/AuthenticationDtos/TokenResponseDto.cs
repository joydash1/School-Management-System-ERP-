using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.DTOs.AuthenticationDtos
{
    public sealed record TokenResponseDto(
        string AccessToken,
        string RefreshToken,
        DateTime TokenExpiry
    );
}