using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Events.PostReacts;

public sealed record PostReactUpdatedDomainEvent(
    int Id,
    ReactType ReactType
) : IDomainEvent;

