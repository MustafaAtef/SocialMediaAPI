using System;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Entities;

public class PostAttachment
{
    public int Id { get; set; }
    public AttachmentType AttachmentType { get; set; }
    public StorageProvider StorageProvider { get; set; }
    public string Url { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; }
}
