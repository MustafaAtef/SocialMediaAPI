using System;
using SocialMedia.Application.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Exceptions;
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
            throw new BadRequestException("No file uploaded.");
        }

        folderName = folderName?.Trim() ?? string.Empty;

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
            throw new BadRequestException("Invalid file format. Only images and videos are allowed.");
        }
    }
    private async Task<(StorageProvider, AttachmentType, string)> _uploadImageAsync(IFormFile file, string folderName)
    {

        if (file is null || file.Length == 0)
        {
            throw new BadRequestException("No image uploaded.");
        }
        if (file.Length > 10 * 1024 * 1024)
        {
            throw new BadRequestException("Image size exceeds the limit of 10 MB.");
        }

        var webRootPath = GetWebRootPath();
        var basePath = folderName != string.Empty
            ? Path.Combine(webRootPath, "uploads", folderName, "images")
            : Path.Combine(webRootPath, "uploads", "images");
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
            throw new BadRequestException("No video uploaded.");
        }
        if (file.Length > 20 * 1024 * 1024)
        {
            throw new BadRequestException("Video size exceeds the limit of 20 MB.");
        }
        var webRootPath = GetWebRootPath();
        var basePath = folderName != string.Empty
            ? Path.Combine(webRootPath, "uploads", folderName, "videos")
            : Path.Combine(webRootPath, "uploads", "videos");
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
        if (string.IsNullOrWhiteSpace(url))
        {
            return Task.CompletedTask;
        }

        var filePath = Path.Combine(GetWebRootPath(), url.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }

    private string GetWebRootPath()
    {
        var webRootPath = _webHostEnvironment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
        }

        if (!Directory.Exists(webRootPath))
        {
            Directory.CreateDirectory(webRootPath);
        }

        return webRootPath;
    }
}
