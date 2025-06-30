using System;
using SocialMedia.Application.ServiceContracts;
using Microsoft.AspNetCore.Http;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Exceptions;

namespace SocialMedia.Infrastructure.FileUploading;

public class SupabaseFileUploader : IFileUploader
{
    private readonly Supabase.Client _supabaseClient;
    public SupabaseFileUploader(Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }



    public async Task<(StorageProvider StorageProvider, AttachmentType attachmentType, string Url)> UploadAsync(IFormFile file, string bucketName = "default")
    {
        if (file == null || file.Length == 0)
        {
            throw new BadRequestException("No file uploaded.");
        }

        var acceptedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var acceptedVideoExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv" };

        if (acceptedImageExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
        {
            return await _uploadImageAsync(file, bucketName);
        }
        else if (acceptedVideoExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
        {
            return await _uploadVideoAsync(file, bucketName);
        }
        else
        {
            throw new BadRequestException("Invalid file format");
        }
    }


    private async Task<(StorageProvider StorageProvider, AttachmentType, string Url)> _uploadImageAsync(IFormFile file, string bucketName)
    {
        if (file is null || file.Length == 0)
        {
            throw new BadRequestException("No image uploaded.");
        }
        if (file.Length > 5 * 1024 * 1024)
        {
            throw new BadRequestException("Image size exceeds the limit of 5 MB.");
        }
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileContent = ms.ToArray();
        var response = await _supabaseClient.Storage.From(bucketName).Upload(fileContent, fileName);
        var url = _supabaseClient.Storage.From(bucketName).GetPublicUrl(fileName);
        if (url is null)
        {
            throw new BadRequestException("Failed to upload image to Supabase.");
        }
        return (StorageProvider.Supabase, AttachmentType.Image, url);
    }

    private async Task<(StorageProvider, AttachmentType, string)> _uploadVideoAsync(IFormFile file, string bucketName)
    {
        if (file is null || file.Length == 0)
        {
            throw new BadRequestException("No video uploaded.");
        }
        if (file.Length > 20 * 1024 * 1024)
        {
            throw new BadRequestException("Video size exceeds the limit of 20 MB.");
        }
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileContent = ms.ToArray();
        var response = await _supabaseClient.Storage.From(bucketName).Upload(fileContent, fileName);
        var url = _supabaseClient.Storage.From(bucketName).GetPublicUrl(fileName);
        if (url is null)
        {
            throw new BadRequestException("Failed to upload video to Supabase.");
        }
        return (StorageProvider.Supabase, AttachmentType.Video, url);
    }

    public Task DeleteAsync(string url)
    {
        var splitedUrl = url.Split('/');
        var filename = splitedUrl[^1];
        var bucketName = splitedUrl[^2];
        return _supabaseClient.Storage.From(bucketName).Remove(filename);
    }
}
