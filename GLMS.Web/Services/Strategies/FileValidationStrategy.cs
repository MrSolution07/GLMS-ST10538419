using Microsoft.AspNetCore.Http;

namespace GLMS.Web.Services.Strategies;

/// <summary>
/// Validates uploaded files: only PDF files are permitted and size is capped at 10 MB.
/// </summary>
public class FileValidationStrategy : IValidationStrategy<IFormFile?>
{
    private static readonly string[] AllowedExtensions = { ".pdf" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public ValidationResult Validate(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return ValidationResult.Failure("Please select a PDF file to upload.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            return ValidationResult.Failure(
                $"Invalid file type '{extension}'. Only PDF files are accepted.");

        if (file.Length > MaxFileSizeBytes)
            return ValidationResult.Failure(
                $"File size ({file.Length / 1024 / 1024} MB) exceeds the 10 MB limit.");

        return ValidationResult.Success();
    }
}
