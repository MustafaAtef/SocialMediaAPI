using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.Posts.Commands.Restore;

public record RestorePostCommand(int PostId) : ICommand<UserPostsDto>
{

}