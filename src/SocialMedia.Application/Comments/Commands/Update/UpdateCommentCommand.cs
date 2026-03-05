using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.Comments.Commands.Update;

public record UpdateCommentCommand(int CommentId, int PostId, string Content) : ICommand<CommentDto>;
