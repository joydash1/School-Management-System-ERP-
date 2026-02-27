using SchoolManagement.Domain.DTOs.AuthenticationAndAuthorizationDtos;
using SchoolManagement.Domain.DTOs.CommonDtos;
using SixLabors.ImageSharp;

namespace SchoolManagement.Infrastructure.Services.CommonServices
{
    public class ProfileService
    {
        public async Task<Result> UpdateProfileAsync(UpdateProfileDto dto)
        {
            if (dto.ProfilePicture != null)
            {
                if (dto.ProfilePicture.Length < 1 * 1024 * 1024 ||
                    dto.ProfilePicture.Length > 2 * 1024 * 1024)
                {
                    return Result.Failure("Profile picture must be between 1MB and 2MB");
                }

                var extension = Path.GetExtension(dto.ProfilePicture.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (!allowedExtensions.Contains(extension))
                {
                    return Result.Failure("Only image files are allowed");
                }
                using var stream = dto.ProfilePicture.OpenReadStream();
                var image = await Image.LoadAsync(stream);

                if (image.Width < 100 || image.Height < 100)
                {
                    return Result.Failure("Image dimensions must be at least 100x100 pixels");
                }
            }
            return Result.Success();
        }
    }
}