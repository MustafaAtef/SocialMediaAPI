using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Posts.Commands.PermanentDelete;

public record PermanentDeletePostCommand(int PostId) : ICommand
{

}