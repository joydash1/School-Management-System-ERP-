using SchoolManagement.Domain.DTOs.AuthenticationAndAuthorizationDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Interfaces.AuthenticationAndAuthorization
{
    public interface IAuthenticationRepository
    {
        // Authentication
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

        Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto googleLoginDto);

        Task LogoutAsync(string userId);

        // Token management
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);

        // Password management
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);

        Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);

        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        // User management
        Task<UserDto> GetUserByIdAsync(string userId);

        Task<UserDto> GetUserByEmailAsync(string email);

        Task<List<UserDto>> GetAllUsersAsync();

        // Role management
        Task<bool> AssignRoleAsync(AssignRoleDto assignRoleDto);

        Task<bool> RemoveRoleAsync(string userId, string roleName);

        Task<List<string>> GetUserRolesAsync(string userId);

        // Account management
        Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto updateProfileDto);

        Task<bool> DeactivateAccountAsync(string userId);

        Task<bool> ActivateAccountAsync(string userId);
    }
}