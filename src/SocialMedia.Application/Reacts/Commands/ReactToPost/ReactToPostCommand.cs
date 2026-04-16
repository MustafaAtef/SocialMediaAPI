using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Reacts.Responses;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.Reacts.Commands.ReactToPost;

public record ReactToPostCommand(int PostId, ReactType ReactType) : ICommand<PostReactResponse>;
