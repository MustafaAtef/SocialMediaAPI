using Microsoft.AspNetCore.Http;

using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Exceptions;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Infrastructure.FileUploading;

public class SupabaseFileUploader(Supabase.Client supabaseClient) : FileUploaderBase
{
    protected override async Task<UploadedFileDto> UploadImageAsync(IFormFile file, string bucketName)
    {
        if (file.Length > MaxImageBytes)
        {
            throw new BadRequestException($"Image size exceeds the limit of {MaxImageBytes / (1024 * 1024)} MB.");
        }
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileContent = ms.ToArray();
        var existingBucket = await supabaseClient.Storage.GetBucket(bucketName);
        if (existingBucket is null)
        {
            await supabaseClient.Storage.CreateBucket(bucketName, new() { Public = true });
        }
        var response = await supabaseClient.Storage.From(bucketName).Upload(fileContent, fileName);
        var url = supabaseClient.Storage.From(bucketName).GetPublicUrl(fileName);
        if (url is null)
        {
            throw new BadRequestException("Failed to upload image to Supabase.");
        }
        return new UploadedFileDto { Type = AttachmentType.Image, StorageProvider = StorageProvider.Supabase, Url = url };
    }

    protected override async Task<UploadedFileDto> UploadVideoAsync(IFormFile file, string bucketName)
    {
        if (file is null || file.Length == 0)
        {
            throw new BadRequestException("No video uploaded.");
        }
        if (file.Length > MaxVideoBytes)
        {
            throw new BadRequestException($"Video size exceeds the limit of {MaxVideoBytes / (1024 * 1024)} MB.");
        }
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileContent = ms.ToArray();
        var existingBucket = await supabaseClient.Storage.GetBucket(bucketName);
        if (existingBucket is null)
        {
            await supabaseClient.Storage.CreateBucket(bucketName, new() { Public = true });
        }
        var response = await supabaseClient.Storage.From(bucketName).Upload(fileContent, fileName);
        var url = supabaseClient.Storage.From(bucketName).GetPublicUrl(fileName);
        if (url is null)
        {
            throw new BadRequestException("Failed to upload video to Supabase.");
        }
        return new UploadedFileDto { Type = AttachmentType.Video, StorageProvider = StorageProvider.Supabase, Url = url };
    }

    public override Task DeleteAsync(string url)
    {
        var splitedUrl = url.Split('/');
        var filename = splitedUrl[^1];
        var bucketName = splitedUrl[^2];
        return supabaseClient.Storage.From(bucketName).Remove(filename);
    }
}
