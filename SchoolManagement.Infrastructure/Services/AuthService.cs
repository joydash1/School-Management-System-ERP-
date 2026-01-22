//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Logging;
//using SchoolManagement.DataAccess.DataContext;
//using SchoolManagement.Domain.DTOs.AuthenticationDtos;
//using SchoolManagement.Domain.DTOs.CommonDtos;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;

//namespace SchoolManagement.Infrastructure.Services
//{
//    public class AuthService : IAuthService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IPasswordHasher _passwordHasher;
//        private readonly IJwtService _jwtService;
//        private readonly ILogger<AuthService> _logger;

//        public AuthService(
//            ApplicationDbContext context,
//            IPasswordHasher passwordHasher,
//            IJwtService jwtService,
//            ILogger<AuthService> logger)
//        {
//            _context = context;
//            _passwordHasher = passwordHasher;
//            _jwtService = jwtService;
//            _logger = logger;
//        }

//        public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto request)
//        {
//            try
//            {
//                // Validate input
//                var validationErrors = ValidateRegistration(request);
//                if (validationErrors.Any())
//                    return Result<AuthResponseDto>.ValidationFailed(validationErrors);

//                // Check if user exists
//                var existingUser = await _context.ApplicationUsers
//                    .FirstOrDefaultAsync(u => u.Email == request.Email || u.EmployeeCode == request.EmployeeCode);

//                if (existingUser != null)
//                {
//                    if (existingUser.Email == request.Email)
//                        return Result<AuthResponseDto>.Conflict("Email already registered");

//                    return Result<AuthResponseDto>.Conflict("Employee code already exists");
//                }

//                // Create password hash
//                _passwordHasher.CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

//                // Create user entity
//                var user = new ApplicationUser
//                {
//                    FullName = request.FullName,
//                    Email = request.Email,
//                    JobTitle = request.JobTitle,
//                    EmployeeCode = request.EmployeeCode,
//                    MobileNo = request.MobileNo,
//                    PasswordHash = passwordHash,
//                    PasswordSalt = passwordSalt,
//                    RoleID = ApplicationUserRole.User,
//                    IsActive = true,
//                    CreatedAt = DateTime.UtcNow
//                };

//                // Handle profile picture if provided
//                if (request.ProfilePicture != null)
//                {
//                    user.ProfilePicturePath = await SaveProfilePictureAsync(request.ProfilePicture);
//                }

//                // Save to database
//                await _context.ApplicationUsers.AddAsync(user);
//                await _context.SaveChangesAsync();

//                // Generate JWT tokens
//                var tokens = await _jwtService.GenerateTokensAsync(user);

//                // Create response
//                var response = new AuthResponseDto(
//                    user.ID,
//                    user.FullName,
//                    user.Email,
//                    user.RoleID.ToString(),
//                    user.ProfilePicturePath,
//                    tokens.AccessToken,
//                    tokens.RefreshToken,
//                    tokens.AccessTokenExpiry
//                );

//                return Result<AuthResponseDto>.Success(
//                    response,
//                    StatusCodes.Status201Created,
//                    "User registered successfully"
//                );
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Registration failed for email: {Email}", request.Email);
//                return Result<AuthResponseDto>.Failure(
//                    "Registration failed. Please try again later.",
//                    StatusCodes.Status500InternalServerError
//                );
//            }
//        }

//        public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto request)
//        {
//            try
//            {
//                // Find user
//                var user = await _context.ApplicationUsers
//                    .FirstOrDefaultAsync(u => u.Email == request.Email);

//                if (user == null)
//                    return Result<AuthResponseDto>.Unauthorized("Invalid email or password");

//                // Verify password
//                if (!_passwordHasher.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
//                    return Result<AuthResponseDto>.Unauthorized("Invalid email or password");

//                // Check if user is active
//                if (!user.IsActive)
//                    return Result<AuthResponseDto>.Forbidden("Account is deactivated");

//                // Generate tokens
//                var tokens = await _jwtService.GenerateTokensAsync(user);

//                // Update last login
//                user.LastLoginAt = DateTime.UtcNow;
//                _context.ApplicationUsers.Update(user);
//                await _context.SaveChangesAsync();

//                // Create response
//                var response = new AuthResponseDto(
//                    user.ID,
//                    user.FullName,
//                    user.Email,
//                    user.RoleID.ToString(),
//                    user.ProfilePicturePath,
//                    tokens.AccessToken,
//                    tokens.RefreshToken,
//                    tokens.AccessTokenExpiry
//                );

//                return Result<AuthResponseDto>.Success(response, "Login successful");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Login failed for email: {Email}", request.Email);
//                return Result<AuthResponseDto>.Failure(
//                    "Login failed. Please try again later.",
//                    StatusCodes.Status500InternalServerError
//                );
//            }
//        }

//        public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
//        {
//            try
//            {
//                // Validate refresh token
//                var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
//                var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

//                var user = await _context.ApplicationUsers
//                    .FirstOrDefaultAsync(u => u.ID == userId);

//                if (user == null)
//                    return Result<AuthResponseDto>.NotFound("User not found");

//                // Validate refresh token in database
//                var storedRefreshToken = await _context.RefreshTokens
//                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken &&
//                                             rt.UserId == userId &&
//                                             !rt.IsRevoked &&
//                                             rt.ExpiryTime > DateTime.UtcNow);

//                if (storedRefreshToken == null)
//                    return Result<AuthResponseDto>.Unauthorized("Invalid refresh token");

//                // Generate new tokens
//                var tokens = await _jwtService.GenerateTokensAsync(user);

//                // Revoke old refresh token
//                storedRefreshToken.IsRevoked = true;
//                storedRefreshToken.RevokedAt = DateTime.UtcNow;
//                _context.RefreshTokens.Update(storedRefreshToken);
//                await _context.SaveChangesAsync();

//                // Create response
//                var response = new AuthResponseDto(
//                    user.ID,
//                    user.FullName,
//                    user.Email,
//                    user.RoleID.ToString(),
//                    user.ProfilePicturePath,
//                    tokens.AccessToken,
//                    tokens.RefreshToken,
//                    tokens.AccessTokenExpiry
//                );

//                return Result<AuthResponseDto>.Success(response, "Token refreshed successfully");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Token refresh failed");
//                return Result<AuthResponseDto>.Failure(
//                    "Token refresh failed",
//                    StatusCodes.Status500InternalServerError
//                );
//            }
//        }

//        public async Task<Result> LogoutAsync(int userId)
//        {
//            try
//            {
//                // Revoke all refresh tokens for this user
//                var refreshTokens = await _context.RefreshTokens
//                    .Where(rt => rt.UserId == userId && !rt.IsRevoked)
//                    .ToListAsync();

//                foreach (var token in refreshTokens)
//                {
//                    token.IsRevoked = true;
//                    token.RevokedAt = DateTime.UtcNow;
//                }

//                _context.RefreshTokens.UpdateRange(refreshTokens);
//                await _context.SaveChangesAsync();

//                return Result.Success("Logged out successfully");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Logout failed for user: {UserId}", userId);
//                return Result.Failure("Logout failed", StatusCodes.Status500InternalServerError);
//            }
//        }

//        private List<string> ValidateRegistration(RegisterDto request)
//        {
//            var errors = new List<string>();

//            if (string.IsNullOrWhiteSpace(request.FullName))
//                errors.Add("Full name is required");

//            if (string.IsNullOrWhiteSpace(request.Email))
//                errors.Add("Email is required");
//            else if (!IsValidEmail(request.Email))
//                errors.Add("Invalid email format");

//            if (string.IsNullOrWhiteSpace(request.Password))
//                errors.Add("Password is required");
//            else if (request.Password.Length < 8)
//                errors.Add("Password must be at least 8 characters");
//            else if (!PasswordMeetsRequirements(request.Password))
//                errors.Add("Password must contain uppercase, lowercase, number and special character");

//            if (string.IsNullOrWhiteSpace(request.EmployeeCode))
//                errors.Add("Employee code is required");

//            if (string.IsNullOrWhiteSpace(request.MobileNo) || request.MobileNo.Length < 11)
//                errors.Add("Valid mobile number is required (at least 11 digits)");

//            return errors;
//        }

//        private bool IsValidEmail(string email)
//        {
//            try
//            {
//                var addr = new System.Net.Mail.MailAddress(email);
//                return addr.Address == email;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        private bool PasswordMeetsRequirements(string password)
//        {
//            return password.Any(char.IsUpper) &&
//                   password.Any(char.IsLower) &&
//                   password.Any(char.IsDigit) &&
//                   password.Any(ch => !char.IsLetterOrDigit(ch));
//        }

//        private async Task<string> SaveProfilePictureAsync(IFormFile file)
//        {
//            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");

//            if (!Directory.Exists(uploadsFolder))
//                Directory.CreateDirectory(uploadsFolder);

//            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
//            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

//            using (var stream = new FileStream(filePath, FileMode.Create))
//            {
//                await file.CopyToAsync(stream);
//            }

//            return $"/uploads/profiles/{uniqueFileName}";
//        }
//    }