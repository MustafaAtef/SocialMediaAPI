using Microsoft.AspNetCore.Http;

using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IFileUploader
{
    Task<UploadedFileDto> UploadAsync(IFormFile file, string? folderName);
    Task DeleteAsync(string url);
}
