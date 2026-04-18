using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.Users.Queries.GetPagedGroupMessages;

public sealed record GetPagedGroupMessagesQuery(
    Guid GroupId,
    int? LastMessageId,
    int OlderMessagesSize)
    : ICurrentUserQuery<GroupMessagesDto>
{
    public int UserId { get; set; }
}