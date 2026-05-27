using GLMS.Web.Services.Strategies;
using Microsoft.AspNetCore.Http;
using Moq;

namespace GLMS.Tests;

/// <summary>
/// Unit tests for FileValidationStrategy — ensures only valid PDF files pass validation
/// and all disallowed types, oversized, or null files are rejected with clear messages.
/// </summary>
public class FileValidationTests
{
    private readonly FileValidationStrategy _sut = new();

    private static IFormFile MakeFakeFile(string fileName, long sizeBytes = 1024)
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.Length).Returns(sizeBytes);
        return mock.Object;
    }

    [Fact(DisplayName = "PDF file passes validation")]
    public void ValidPdf_ReturnsSuccess()
    {
        var file   = MakeFakeFile("contract.pdf");
        var result = _sut.Validate(file);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = ".exe file is rejected")]
    public void ExeFile_ReturnsFailure()
    {
        var file   = MakeFakeFile("malware.exe");
        var result = _sut.Validate(file);

        Assert.False(result.IsValid);
        Assert.Contains(".exe", result.ErrorMessage);
    }

    [Fact(DisplayName = ".docx file is rejected — only PDF allowed")]
    public void DocxFile_ReturnsFailure()
    {
        var file   = MakeFakeFile("document.docx");
        var result = _sut.Validate(file);

        Assert.False(result.IsValid);
        Assert.Contains(".docx", result.ErrorMessage);
    }

    [Fact(DisplayName = ".jpg image file is rejected")]
    public void JpgFile_ReturnsFailure()
    {
        var file   = MakeFakeFile("photo.jpg");
        var result = _sut.Validate(file);

        Assert.False(result.IsValid);
    }

    [Fact(DisplayName = "Null file returns failure")]
    public void NullFile_ReturnsFailure()
    {
        var result = _sut.Validate(null);

        Assert.False(result.IsValid);
    }

    [Fact(DisplayName = "Empty file (zero bytes) is rejected")]
    public void EmptyFile_ReturnsFailure()
    {
        var file   = MakeFakeFile("empty.pdf", sizeBytes: 0);
        var result = _sut.Validate(file);

        Assert.False(result.IsValid);
    }

    [Fact(DisplayName = "File exceeding 10 MB limit is rejected")]
    public void OversizedFile_ReturnsFailure()
    {
        long elevenMb = 11L * 1024 * 1024;
        var  file     = MakeFakeFile("huge.pdf", sizeBytes: elevenMb);
        var  result   = _sut.Validate(file);

        Assert.False(result.IsValid);
        Assert.Contains("MB", result.ErrorMessage);
    }

    [Fact(DisplayName = "File exactly at 10 MB limit passes")]
    public void ExactlyAtLimit_ReturnsSuccess()
    {
        long tenMb = 10L * 1024 * 1024;
        var  file  = MakeFakeFile("bigcontract.pdf", sizeBytes: tenMb);
        var  result = _sut.Validate(file);

        Assert.True(result.IsValid);
    }

    [Theory(DisplayName = "Various disallowed extensions are rejected")]
    [InlineData("script.bat")]
    [InlineData("archive.zip")]
    [InlineData("image.png")]
    [InlineData("spreadsheet.xlsx")]
    [InlineData("video.mp4")]
    public void DisallowedExtensions_AreRejected(string fileName)
    {
        var file   = MakeFakeFile(fileName);
        var result = _sut.Validate(file);

        Assert.False(result.IsValid);
    }
}
