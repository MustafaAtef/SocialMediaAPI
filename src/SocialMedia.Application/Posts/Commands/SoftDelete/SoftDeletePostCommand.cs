using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Posts.Commands.SoftDelete;

public record SoftDeletePostCommand(int PostId) : ICommand
{

}
