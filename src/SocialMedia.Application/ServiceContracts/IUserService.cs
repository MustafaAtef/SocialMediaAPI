using System;
using System.Security.Claims;

using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IUserService
{
    UserDto? GetAuthenticatedUser(ClaimsPrincipal? claimsPrincipal = null);
    Task<ICollection<GroupMessagesDto>> GetAllGroupMessagesAsync(int lastMessagesSize);
    Task<GroupMessagesDto> GetPagedGroupMessagesAsync(Guid groupId, int? lastMessageId, int olderMessagesSize);
}
