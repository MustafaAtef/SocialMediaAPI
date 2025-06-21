using System;
using SocialMedia.Application.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
namespace SocialMedia.Infrastructure.FileUploading;

public class ServerFileUploader : IFileUploader
{

    private readonly IWebHostEnvironment _webHostEnvironment;
    public ServerFileUploader(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<(string, string)> UploadImageAsync(IFormFile file, string folderName = "")
    {
        var acceptedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("No image uploaded.", nameof(file));
        }
        if (file.Length > 10 * 1024 * 1024)
        {
            throw new ArgumentException("Image size exceeds the limit of 10 MB.", nameof(file));
        }
        if (!acceptedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
        {
            throw new ArgumentException("Invalid image format. Accepted formats are: jpg, jpeg, png, gif.", nameof(file));
        }
        var basePath = folderName != "" ? Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folderName) : Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(basePath, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return ("server", folderName == ""
            ? $"/uploads/{fileName}"
            : $"/uploads/{folderName}/{fileName}");
    }

    public Task DeleteImageAsync(string url)
    {
        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, url.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }
}
