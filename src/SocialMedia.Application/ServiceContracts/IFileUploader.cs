using System;
using Microsoft.AspNetCore.Http;

namespace SocialMedia.Application.ServiceContracts;

public interface IFileUploader
{
    Task<(string StorageProvider, string Url)> UploadImageAsync(IFormFile file, string folderName);
    Task DeleteImageAsync(string url);
}
