using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Events.CommentReacts;

public sealed record CommentReactUpdatedDomainEvent(
    int Id,
    ReactType ReactType
) : IDomainEvent;

