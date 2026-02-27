using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SchoolManagement.DataAccess.DataContext;
using SchoolManagement.DataAccess.UnitOfWork;
using SchoolManagement.Domain.DTOs.AuthenticationAndAuthorizationDtos;
using SchoolManagement.Domain.Entities.Authentication;
using SchoolManagement.Domain.Interfaces.AuthenticationAndAuthorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DataAccess.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUsers> _userManager;
        private readonly SignInManager<ApplicationUsers> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthenticationRepository(IUnitOfWork unitOfWork, UserManager<ApplicationUsers> userManager, SignInManager<ApplicationUsers> signInManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        #region Authentication Methods

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Check if user exists
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Registration failed",
                        Errors = new List<string> { "Email already registered" }
                    };
                }

                // Create new user
                var user = new ApplicationUsers
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true // Set to false if you want email confirmation
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Registration failed",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                // Assign role
                var role = registerDto.Role ?? "Student";
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                await _userManager.AddToRoleAsync(user, role);

                // Generate tokens
                var (token, refreshToken) = await GenerateTokensAsync(user);

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);

                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Registration successful",
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(
                        Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"] ?? "60")),
                    User = MapToUserDto(user, roles.ToList())
                };
            });
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Find user by email
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Invalid login attempt",
                        Errors = new List<string> { "Invalid email or password" }
                    };
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Account is deactivated",
                        Errors = new List<string> { "Your account has been deactivated. Please contact administrator." }
                    };
                }

                // Check password
                var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, loginDto.RememberMe, lockoutOnFailure: false);

                if (result.IsLockedOut)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Account locked",
                        Errors = new List<string> { "Account is locked due to multiple failed attempts. Try again later." }
                    };
                }

                if (!result.Succeeded)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Invalid login attempt",
                        Errors = new List<string> { "Invalid email or password" }
                    };
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Generate tokens
                var (token, refreshToken) = await GenerateTokensAsync(user);

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);

                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Login successful",
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(
                        Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"] ?? "60")),
                    User = MapToUserDto(user, roles.ToList())
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Login failed",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto googleLoginDto)
        {
            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Google login initiated"
            };
        }

        public async Task LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.SignOutAsync();
        }

        #endregion Authentication Methods

        #region Token Management

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var principal = GetPrincipalFromExpiredToken(refreshTokenDto.Token);
                if (principal == null)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Invalid token",
                        Errors = new List<string> { "Invalid access token" }
                    };
                }

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null ||
                    user.RefreshToken != refreshTokenDto.RefreshToken ||
                    user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Invalid refresh token",
                        Errors = new List<string> { "Refresh token is invalid or expired" }
                    };
                }

                // Generate new tokens
                var (newToken, newRefreshToken) = await GenerateTokensAsync(user);

                var roles = await _userManager.GetRolesAsync(user);

                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Token refreshed successfully",
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    ExpiryDate = DateTime.UtcNow.AddMinutes(
                        Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"] ?? "60")),
                    User = MapToUserDto(user, roles.ToList())
                };
            });
        }

        #endregion Token Management

        #region Password Management

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                var result = await _userManager.ChangePasswordAsync(
                    user,
                    changePasswordDto.CurrentPassword,
                    changePasswordDto.NewPassword);

                return result.Succeeded;
            });
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null) return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null) return false;

            var result = await _userManager.ResetPasswordAsync(
                user,
                resetPasswordDto.Token,
                resetPasswordDto.Password);

            return result.Succeeded;
        }

        #endregion Password Management

        #region User Management

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return MapToUserDto(user, roles.ToList());
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return MapToUserDto(user, roles.ToList());
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(MapToUserDto(user, roles.ToList()));
            }

            return userDtos;
        }

        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto updateProfileDto)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                user.FirstName = updateProfileDto.FirstName ?? user.FirstName;
                user.LastName = updateProfileDto.LastName ?? user.LastName;
                //user.PhoneNumber = updateProfileDto.PhoneNumber ?? user.PhoneNumber;
                //user.Address = updateProfileDto.Address ?? user.Address;
                //user.DateOfBirth = updateProfileDto.DateOfBirth ?? user.DateOfBirth;

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            });
        }

        public async Task<bool> DeactivateAccountAsync(string userId)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                user.IsActive = false;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            });
        }

        public async Task<bool> ActivateAccountAsync(string userId)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                user.IsActive = true;
                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            });
        }

        #endregion User Management

        #region Role Management

        public async Task<bool> AssignRoleAsync(AssignRoleDto assignRoleDto)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.FindByIdAsync(assignRoleDto.UserId);
                if (user == null) return false;

                if (!await _roleManager.RoleExistsAsync(assignRoleDto.RoleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(assignRoleDto.RoleName));
                }

                var result = await _userManager.AddToRoleAsync(user, assignRoleDto.RoleName);
                return result.Succeeded;
            });
        }

        public async Task<bool> RemoveRoleAsync(string userId, string roleName)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                return result.Succeeded;
            });
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        #endregion Role Management

        #region Private Helper Methods

        private async Task<(string token, string refreshToken)> GenerateTokensAsync(ApplicationUsers user)
        {
            var token = await GenerateJwtTokenAsync(user);
            var refreshToken = GenerateRefreshToken();

            // Save refresh token to user
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return (token, refreshToken);
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUsers user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
                new Claim("fullName", user.FullName)
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiryMinutes = Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");
            var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"] ?? "school-management-system",
                audience: _configuration["JwtSettings:Audience"] ?? "school-management-client",
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _configuration["JwtSettings:SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long")),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;

                if (jwtSecurityToken == null ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private UserDto MapToUserDto(ApplicationUsers user, List<string> roles)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };
        }

        #endregion Private Helper Methods
    }
}