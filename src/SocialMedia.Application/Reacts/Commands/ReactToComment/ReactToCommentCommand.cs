using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Reacts.Responses;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.Reacts.Commands.ReactToComment;

public record ReactToCommentCommand(int PostId, int CommentId, ReactType ReactType) : ICommand<CommentReactResponse>;
