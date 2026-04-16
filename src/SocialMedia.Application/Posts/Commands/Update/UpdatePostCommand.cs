using Microsoft.AspNetCore.Http;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Posts.Responses;

namespace SocialMedia.Application.Posts.Commands.Update;

public record UpdatePostCommand(
    int PostId,
    string? Content,
    List<IFormFile>? AddedAttachments,
    List<int>? DeletedAttachmentIds
) : ICommand<PostResponse>
{

}
