using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.DTOs.AuthenticationDtos
{
    public sealed record AuthResponseDto(
        Guid UserId,
        string FullName,
        string Email,
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiresAt
    );
}