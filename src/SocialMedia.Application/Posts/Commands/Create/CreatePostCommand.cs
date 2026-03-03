
using Microsoft.AspNetCore.Http;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.Posts.Commands;

public record CreatePostCommand(
    string Content,
    List<IFormFile> Attachments) : ICommand<PostDto>
{

}
