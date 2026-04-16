using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Posts.Responses;

namespace SocialMedia.Application.Posts.Commands.Restore;

public record RestorePostCommand(int PostId) : ICommand<PostResponse>
{

}