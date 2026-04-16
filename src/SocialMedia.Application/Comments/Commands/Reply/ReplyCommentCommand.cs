using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Comments.Responses;

namespace SocialMedia.Application.Comments.Commands.Reply;

public record ReplyCommentCommand(int PostId, int ParentCommentId, string Content) : ICommand<CommentResponse>;
