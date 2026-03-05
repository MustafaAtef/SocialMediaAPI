using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.Reacts.Commands.ReactToComment;

public record ReactToCommentCommand(int PostId, int CommentId, ReactType ReactType) : ICommand<CommentReactDto>;
