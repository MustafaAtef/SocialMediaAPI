using EducationCenter.Core.RepositoryContracts;
using SocialMedia.Infrastructure.Database;
using SocialMedia.Core.Entities;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Core.Enumerations;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }
    private AppDbContext _appDbContext => (AppDbContext)_dbContext;
    public async Task UpdateSentMessagesToDelivered(int userId)
    {
        await _appDbContext.Set<MessageStatus>().Where(ms => ms.RecieverId == userId).ExecuteUpdateAsync(set => set.SetProperty(p => p.Status, MessageStatusType.Delivered).SetProperty(p => p.DeliveredAt, DateTime.Now));
    }
    public async Task UpdateDeliveredMessagesToSeen(int recieverId, Guid groupId)
    {
        await _appDbContext.Set<MessageStatus>().Where(ms => ms.RecieverId == recieverId && ms.Message.GroupId == groupId).ExecuteUpdateAsync(set => set.SetProperty(p => p.Status, MessageStatusType.Seen).SetProperty(p => p.SeenAt, DateTime.Now));
    }

    public async Task<User?> GetAllGroupsMessagesAsync(int userId, int olderMessagesSize)
    {
        var user = await _appDbContext.Set<User>()
        .Where(u => u.Id == userId)
        .Include(u => u.Groups)
        .ThenInclude(g => g.Users)
        .ThenInclude(u => u.Avatar)
        .Include(u => u.Groups)
        .ThenInclude(g => g.Messages.OrderByDescending(m => m.CreatedAt).Take(olderMessagesSize))
        .ThenInclude(m => m.MessageStatuses)
        .AsSplitQuery()
        .ToListAsync();
        if (user.Count < 1) return null;
        return user[0];
    }

    public async Task<User?> GetPagedGroupMessagesAsync(int userId, Guid groupId, int LastMessageId, int olderMessagesSize)
    {
        var user = await _appDbContext.Set<User>()
        .Where(u => u.Id == userId)
        .Include(u => u.Groups.Where(g => g.Id == groupId))
        .ThenInclude(g => g.Users)
        .ThenInclude(u => u.Avatar)
        .Include(u => u.Groups)
        .ThenInclude(g => g.Messages.Where(m => m.Id < LastMessageId).OrderByDescending(m => m.CreatedAt).Take(olderMessagesSize))
        .ThenInclude(m => m.MessageStatuses)
        .AsSplitQuery()
        .ToListAsync();
        if (user.Count < 1) return null;
        return user[0];
    }
}
