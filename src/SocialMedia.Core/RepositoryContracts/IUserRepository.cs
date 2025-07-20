using System;
using SocialMedia.Core.Entities;
using SocialMedia.Core.RepositoryContracts;

namespace EducationCenter.Core.RepositoryContracts;

public interface IUserRepository : IRepository<User>
{
    Task UpdateSentMessagesToDelivered(int userId);
    Task UpdateDeliveredMessagesToSeen(int RecieverId, Guid groupId);
    Task<User?> GetAllGroupsMessagesAsync(int userId, int olderMessagesSize);
    Task<User?> GetPagedGroupMessagesAsync(int userId, Guid groupId, int LastMessageId, int olderMessagesSize);
}
