using System;

using Microsoft.AspNetCore.Http;

using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Exceptions;

namespace SocialMedia.Infrastructure.FileUploading;

public abstract class FileUploaderBase : IFileUploader
{

    protected static readonly HashSet<string> ImageExtensions = [".jpg", ".jpeg", ".png", ".gif"];
    protected static readonly HashSet<string> VideoExtensions = [".mp4", ".avi", ".mov", ".mkv"];

    protected const long MaxImageBytes = 10 * 1024 * 1024;
    protected const long MaxVideoBytes = 20 * 1024 * 1024;

    public async Task<UploadedFileDto> UploadAsync(IFormFile file, string? folderName)
    {
        if (file is null || file.Length == 0)
            throw new BadRequestException("No file uploaded.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        folderName = folderName?.Trim() ?? string.Empty;

        if (ImageExtensions.Contains(ext)) return await UploadImageAsync(file, folderName);
        if (VideoExtensions.Contains(ext)) return await UploadVideoAsync(file, folderName);

        throw new BadRequestException("Invalid file format. Only images and videos are allowed.");
    }

    protected abstract Task<UploadedFileDto> UploadImageAsync(IFormFile file, string folder);
    protected abstract Task<UploadedFileDto> UploadVideoAsync(IFormFile file, string folder);
    public abstract Task DeleteAsync(string url);
}