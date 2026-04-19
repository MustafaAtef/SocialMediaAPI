using System;

using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.Dtos;

public class UploadedFileDto
{
    public AttachmentType Type { get; set; }
    public StorageProvider StorageProvider { get; set; }
    public string Url { get; set; } = string.Empty;
}
