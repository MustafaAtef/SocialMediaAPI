using System;
using SocialMedia.Application.ServiceContracts;
using Microsoft.AspNetCore.Http;

namespace SocialMedia.Infrastructure.FileUploading;

public class SupabaseFileUploader : IFileUploader
{
    private readonly Supabase.Client _supabaseClient;
    public SupabaseFileUploader(Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<(string StorageProvider, string Url)> UploadImageAsync(IFormFile file, string bucketName)
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
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileContent = ms.ToArray();
        var response = await _supabaseClient.Storage.From(bucketName).Upload(fileContent, fileName);
        var url = _supabaseClient.Storage.From(bucketName).GetPublicUrl(fileName);
        if (url is null)
        {
            throw new Exception("Failed to upload image to Supabase.");
        }
        return ("supabase", url);
    }

    public Task DeleteImageAsync(string url)
    {
        var splitedUrl = url.Split('/');
        var filename = splitedUrl[^1];
        var bucketName = splitedUrl[^2];
        return _supabaseClient.Storage.From(bucketName).Remove(filename);
    }
}
