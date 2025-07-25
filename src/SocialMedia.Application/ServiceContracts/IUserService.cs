using System;
using System.Security.Claims;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IUserService
{
    UserDto? GetAuthenticatedUser(ClaimsPrincipal? claimsPrincipal = null);
    Task FollowAsync(int userId);
    Task UnFollowAsync(int userId);
    Task<PagedList<UserDto>> GetPagedFollowersAsync(int userId, int page, int pageSize);
    Task<PagedList<UserDto>> GetPagedFollowingsAsync(int userId, int page, int pageSize);
    Task<ICollection<GroupMessagesDto>> GetAllGroupMessagesAsync(int lastMessagesSize);
    Task<GroupMessagesDto> GetPagedGroupMessagesAsync(Guid groupId, int? lastMessageId, int olderMessagesSize);
    Task<UserDto> UpdateAsync(UpdateUserDto updateUserDto);
}
