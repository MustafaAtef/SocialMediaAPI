using System;
using SocialMedia.Application.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using SocialMedia.Core.Enumerations;
using Microsoft.Identity.Client.Extensions.Msal;
namespace SocialMedia.Infrastructure.FileUploading;

public class ServerFileUploader : IFileUploader
{

    private readonly IWebHostEnvironment _webHostEnvironment;
    public ServerFileUploader(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<(StorageProvider, AttachmentType, string)> UploadAsync(IFormFile file, string folderName = "")
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No file uploaded.", nameof(file));
        }

        var acceptedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var acceptedVideoExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv" };

        if (acceptedImageExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
        {
            return await _uploadImageAsync(file, folderName);
        }
        else if (acceptedVideoExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
        {
            return await _uploadVideoAsync(file, folderName);
        }
        else
        {
            throw new ArgumentException("Invalid file format", nameof(file));
        }
    }
    private async Task<(StorageProvider, AttachmentType, string)> _uploadImageAsync(IFormFile file, string folderName)
    {

        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("No image uploaded.", nameof(file));
        }
        if (file.Length > 10 * 1024 * 1024)
        {
            throw new ArgumentException("Image size exceeds the limit of 10 MB.", nameof(file));
        }

        var wwwrootPath = _webHostEnvironment.WebRootPath;
        var basePath = folderName != "" ? Path.Combine(wwwrootPath, "uploads", folderName, "images") : Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "images");
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

        return (StorageProvider.Disk, AttachmentType.Image, folderName == ""
            ? $"/uploads/images/{fileName}"
            : $"/uploads/{folderName}/images/{fileName}");
    }
    private async Task<(StorageProvider StorageProvider, AttachmentType attachmentType, string Url)> _uploadVideoAsync(IFormFile file, string folderName)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("No video uploaded.", nameof(file));
        }
        if (file.Length > 20 * 1024 * 1024)
        {
            throw new ArgumentException("Video size exceeds the limit of 20 MB.", nameof(file));
        }
        var wwwrootPath = _webHostEnvironment.WebRootPath;
        var basePath = folderName != "" ? Path.Combine(wwwrootPath, "uploads", folderName, "videos") : Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "videos");
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

        return (StorageProvider.Disk, AttachmentType.Video, folderName == ""
            ? $"/uploads/videos/{fileName}"
            : $"/uploads/{folderName}/videos/{fileName}");
    }
    public Task DeleteAsync(string url)
    {
        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, url.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }
}
