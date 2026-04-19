using System;
using SocialMedia.Application.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Exceptions;
using SocialMedia.Application.Dtos;
namespace SocialMedia.Infrastructure.FileUploading;

public class ServerFileUploader(IWebHostEnvironment webHostEnvironment) : FileUploaderBase
{
    protected override async Task<UploadedFileDto> UploadImageAsync(IFormFile file, string folderName)
    {
        if (file.Length > MaxImageBytes)
        {
            throw new BadRequestException($"Image size exceeds the limit of {MaxImageBytes / (1024 * 1024)} MB.");
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

        return new UploadedFileDto
        {
            Type = AttachmentType.Image,
            StorageProvider = StorageProvider.Server,
            Url = folderName == ""
            ? $"/uploads/images/{fileName}"
            : $"/uploads/{folderName}/images/{fileName}"
        };
    }

    protected override async Task<UploadedFileDto> UploadVideoAsync(IFormFile file, string folderName)
    {
        if (file.Length > MaxVideoBytes)
        {
            throw new BadRequestException($"Video size exceeds the limit of {MaxVideoBytes / (1024 * 1024)} MB.");
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

        return new UploadedFileDto
        {
            Type = AttachmentType.Video,
            StorageProvider = StorageProvider.Server,
            Url = folderName == ""
            ? $"/uploads/videos/{fileName}"
            : $"/uploads/{folderName}/videos/{fileName}"
        };
    }

    public override Task DeleteAsync(string url)
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
        var webRootPath = webHostEnvironment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");
        }

        if (!Directory.Exists(webRootPath))
        {
            Directory.CreateDirectory(webRootPath);
        }

        return webRootPath;
    }
}
