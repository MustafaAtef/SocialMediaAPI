using Microsoft.AspNetCore.Http;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Posts.Responses;

namespace SocialMedia.Application.Posts.Commands.Create;

public record CreatePostCommand(
    string Content,
    List<IFormFile> Attachments) : ICommand<PostResponse>
{

}
