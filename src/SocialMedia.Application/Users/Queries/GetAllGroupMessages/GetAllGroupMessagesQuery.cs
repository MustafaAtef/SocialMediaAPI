using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.Users.Queries.GetAllGroupMessages;

public sealed record GetAllGroupMessagesQuery(int OlderMessagesSize)
    : ICurrentUserQuery<ICollection<GroupMessagesDto>>
{
    public int UserId { get; set; }
}