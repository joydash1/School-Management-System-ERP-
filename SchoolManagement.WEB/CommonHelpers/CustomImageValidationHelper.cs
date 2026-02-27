using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.WEB.CommonHelpers
{
    public class ImageValidationAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        private readonly int _minFileSize;
        private readonly string[] _allowedExtensions;

        public ImageValidationAttribute(
            int maxFileSize = 2 * 1024 * 1024,
            int minFileSize = 1 * 1024 * 1024,
            string[]? allowedExtensions = null)
        {
            _maxFileSize = maxFileSize;
            _minFileSize = minFileSize;
            _allowedExtensions = allowedExtensions ?? new[] { ".jpg", ".jpeg", ".png", ".gif" };
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                // Check if file is empty
                if (file.Length == 0)
                {
                    return new ValidationResult("The file is empty.");
                }

                // Check minimum size (optional)
                if (_minFileSize > 0 && file.Length < _minFileSize)
                {
                    return new ValidationResult($"File size must be at least {_minFileSize / (1024 * 1024)}MB.");
                }

                // Check maximum size
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult($"File size must not exceed {_maxFileSize / (1024 * 1024)}MB.");
                }

                // Check extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    return new ValidationResult($"Only {string.Join(", ", _allowedExtensions)} files are allowed.");
                }

                // Optional: Validate content type
                var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    return new ValidationResult("Invalid image format.");
                }
            }

            return ValidationResult.Success;
        }
    }
}