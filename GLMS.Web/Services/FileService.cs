using Microsoft.AspNetCore.Http;

namespace GLMS.Web.Services;

/// <summary>
/// Handles saving PDF files to the server's wwwroot/uploads folder.
/// UUIDs prevent filename collisions; the original name is preserved for display.
/// </summary>
public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileService> _logger;
    private const string UploadFolder = "uploads";

    public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
    {
        _env    = env;
        _logger = logger;
    }

    public async Task<(string storedPath, string originalFileName)> SaveSignedAgreementAsync(IFormFile file)
    {
        var uploadDir = Path.Combine(_env.WebRootPath, UploadFolder);
        Directory.CreateDirectory(uploadDir);

        var uniqueName  = $"{Guid.NewGuid():N}.pdf";
        var destination = Path.Combine(uploadDir, uniqueName);

        await using var stream = new FileStream(destination, FileMode.Create);
        await file.CopyToAsync(stream);

        _logger.LogInformation("Saved signed agreement '{Original}' as '{Unique}'.",
            file.FileName, uniqueName);

        return (Path.Combine(UploadFolder, uniqueName), file.FileName);
    }

    public void DeleteFile(string storedPath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, storedPath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted file '{Path}'.", storedPath);
        }
    }

    public bool FileExists(string storedPath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, storedPath);
        return File.Exists(fullPath);
    }
}
