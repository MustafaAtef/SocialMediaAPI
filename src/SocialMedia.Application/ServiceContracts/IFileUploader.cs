using System;
using Microsoft.AspNetCore.Http;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.ServiceContracts;

public interface IFileUploader
{
    Task<(StorageProvider StorageProvider, AttachmentType attachmentType, string Url)> UploadAsync(IFormFile file, string folderName);
    Task DeleteAsync(string url);
}
