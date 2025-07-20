using EducationCenter.Core.RepositoryContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.WebApi.Hubs;

public class ChatHub : Hub
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    public ChatHub(IUnitOfWork unitOfWork, IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
    }
    public override async Task OnConnectedAsync()
    {
        var tokenUser = _userService.GetAuthenticatedUser(Context.User);
        if (tokenUser == null) throw new HubException("Not authenticated user");
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == tokenUser.Id, ["Groups", "UserConnections"]);
        if (user is null) throw new HubException("User not found.");
        bool userFirstConnection = user.UserConnections.Count == 0;
        _unitOfWork.UserConnections.Add(new()
        {
            ConnectionId = Context.ConnectionId,
            UserId = tokenUser.Id
        });
        await _unitOfWork.SaveChangesAsync();
        // send to all groups that that connected user has connected and update all sent messages to delivered (if it's the first connection)
        if (userFirstConnection)
            await _unitOfWork.Users.UpdateSentMessagesToDelivered(tokenUser.Id);
        foreach (var group in user.Groups)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group.Id.ToString());
            if (userFirstConnection)
                await Clients.Group(group.Id.ToString()).SendAsync("DeliveredMessages", new DeliveredMessagesDto()
                {
                    GroudId = group.Id,
                    RecieverId = user.Id
                });
        }
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tokenUser = _userService.GetAuthenticatedUser(Context.User);
        if (tokenUser is null) return;
        _unitOfWork.UserConnections.Remove(new()
        {
            ConnectionId = Context.ConnectionId,
            UserId = tokenUser.Id
        });
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SendDirectMessage(SendDirectMessageDto SendDirectMessageDto)
    {
        var tokenUser = _userService.GetAuthenticatedUser(Context.User);
        if (tokenUser is null) throw new HubException("Not authenticated user.");
        var group = await _unitOfWork.Groups.GetAsync(g => g.Id == SendDirectMessageDto.GroupId, ["Users.UserConnections", "Users.Avatar"]);
        if (group is null) throw new HubException("Group not found.");
        await _saveAndBroadcastMessage(group, tokenUser, SendDirectMessageDto.Message);
    }
    public async Task SendFirstDirectMessage(SendFirstDirectMessageDto SendFirstDirectMessageDto)
    {
        var tokenUser = _userService.GetAuthenticatedUser(Context.User);
        if (tokenUser is null) throw new HubException("Not authenticated user.");
        var sender = await _unitOfWork.Users.GetAsync(u => u.Id == tokenUser.Id);
        var reciever = await _unitOfWork.Users.GetAsync(u => u.Id == SendFirstDirectMessageDto.ToId, ["UserConnections", "Avatar"]);
        if (reciever is null) throw new HubException("Reciever not found.");
        var group = new Group
        {
            Type = GroupType.Direct,
            Users = new List<User>()
            {
                reciever,
                sender
            }
        };
        _unitOfWork.Groups.Add(group);
        await _unitOfWork.SaveChangesAsync();
        await Groups.AddToGroupAsync(Context.ConnectionId, group.Id.ToString());
        if (reciever.UserConnections.Count > 0)
        {
            foreach (var con in reciever.UserConnections)
            {
                await Groups.AddToGroupAsync(con.ConnectionId, group.Id.ToString());
            }
        }
        await _saveAndBroadcastMessage(group, tokenUser, SendFirstDirectMessageDto.Message);

    }

    public async Task ReadMessagesInGroup(ReadMessagesInGroupDto readMessagesInGroupDto)
    {
        await _unitOfWork.Users.UpdateDeliveredMessagesToSeen(readMessagesInGroupDto.RecieverId, readMessagesInGroupDto.GroupId);
        await Clients.Group(readMessagesInGroupDto.GroupId.ToString()).SendAsync("SeenMessages", readMessagesInGroupDto);
    }

    private async Task _saveAndBroadcastMessage(Group group, UserDto tokenUser, string message)
    {
        var msg = new Message
        {
            FromId = tokenUser.Id,
            Data = message,
            MessageStatuses = new List<MessageStatus>()
        };
        foreach (var user in group.Users)
        {
            if (user.Id == tokenUser.Id) continue;
            msg.MessageStatuses.Add(new()
            {
                SentAt = DateTime.Now,
                DeliveredAt = DateTime.Now,
                Reciever = user,
                Status = user.UserConnections.Count > 0 ? MessageStatusType.Delivered : MessageStatusType.Sent
            });
        }
        group.Messages.Add(msg);
        await _unitOfWork.SaveChangesAsync();
        await Clients.Groups(group.Id.ToString()).SendAsync("NewMessage", new MessageDto
        {
            Id = msg.Id,
            GroupId = group.Id,
            SentBy = tokenUser,
            Message = message,
            CreatedAt = msg.CreatedAt,
            Status = msg.MessageStatuses.Select(ms => new MessageStatusDto
            {
                RecievedBy = new UserDto()
                {
                    Id = ms.Reciever.Id,
                    Name = ms.Reciever.FirstName + " " + ms.Reciever.LastName,
                    Email = ms.Reciever.Email,
                    AvatarUrl = ms.Reciever.Avatar?.Url ?? ""
                },
                StatusType = ms.Status,
                Status = ms.Status.ToString(),
                SentAt = ms.SentAt,
                DeliveredAt = ms.DeliveredAt,
                SeenAt = ms.SeenAt
            }).ToList()
        });
    }
}
