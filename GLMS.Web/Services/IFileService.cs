using Microsoft.AspNetCore.Http;

namespace GLMS.Web.Services;

public interface IFileService
{
    Task<(string storedPath, string originalFileName)> SaveSignedAgreementAsync(IFormFile file);
    void DeleteFile(string storedPath);
    bool FileExists(string storedPath);
}
