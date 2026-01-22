using SchoolManagement.Domain.DTOs.AuthenticationDtos;
using SchoolManagement.Domain.DTOs.CommonDtos;
using SchoolManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Interfaces
{
    public interface IAuthRepositories
    {
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);

        Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request);

        Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);

        Task<Result> LogoutAsync(int userId);

        Task<Result<UserProfileDto>> GetUserProfileAsync(int userId);
    }
}