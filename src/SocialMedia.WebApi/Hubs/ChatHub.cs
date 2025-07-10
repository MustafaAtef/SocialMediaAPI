using System.Threading.Tasks;
using EducationCenter.Core.RepositoryContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.WebApi.Hubs;

[Authorize]
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
        if (tokenUser == null) throw new Exception();
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == tokenUser.Id, ["Groups"]);
        if (user is null) throw new Exception();
        _unitOfWork.UserConnections.Add(new()
        {
            ConnectionId = Context.ConnectionId,
            UserId = tokenUser.Id
        });
        foreach (var group in user.Groups)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group.Id.ToString());
        }
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.Users.UpdateSentMessagesToDelivered(tokenUser.Id);
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

    public async Task SendDirectMessage(CreateDirectMessageDto createDirectMessageDto)
    {
        var tokenUser = _userService.GetAuthenticatedUser(Context.User);
        if (tokenUser is null) throw new Exception();
        var group = await _unitOfWork.Groups.GetAsync(g => g.Id == createDirectMessageDto.GroupId, ["Users.UserConnections"]);
        if (group is null) throw new Exception();
        await _saveAndBroadcastDirectMessage(group, tokenUser, createDirectMessageDto.Message);
    }
    public async Task SendFirstDirectMessage(CreateFirstDirectMessageDto createFirstDirectMessageDto)
    {
        var tokenUser = _userService.GetAuthenticatedUser(Context.User);
        if (tokenUser is null) throw new Exception();
        var reciever = await _unitOfWork.Users.GetAsync(u => u.Id == createFirstDirectMessageDto.ToId, ["UserConnections"]);
        if (reciever is null) throw new Exception();
        var group = new Group
        {
            Type = GroupType.Direct,
            Users = new List<User>()
            {
                reciever,
                new User {
                    Id = tokenUser.Id
                }
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
        await _saveAndBroadcastDirectMessage(group, tokenUser, createFirstDirectMessageDto.Message);

    }
    private async Task _saveAndBroadcastDirectMessage(Group group, UserDto tokenUser, string message)
    {
        var msg = new Message
        {
            FromId = tokenUser.Id,
            Data = message,
            MessageStatuses = new List<MessageStatus>()
        };
        bool isOnline = false;
        foreach (var user in group.Users)
        {
            if (user.Id == tokenUser.Id) continue;
            isOnline = user.UserConnections.Count > 0;
            msg.MessageStatuses.Add(new()
            {
                SentAt = DateTime.Now,
                DeliveredAt = DateTime.Now,
                Reciever = user,
                Status = MessageStatusType.Delivered
            });
        }
        group.Messages.Add(msg);
        await _unitOfWork.SaveChangesAsync();
        await Clients.Groups(group.Id.ToString()).SendAsync("NewDirectMessage", new DirectMessageDto
        {
            Id = msg.Id,
            GroupId = group.Id,
            FromUser = new UserDto
            {
                Id = tokenUser.Id,
                Name = tokenUser.Name,
                Email = tokenUser.Email,
                AvatarUrl = tokenUser.AvatarUrl
            },
            StatusType = isOnline ? MessageStatusType.Delivered : MessageStatusType.Sent,
            Status = isOnline ? MessageStatusType.Delivered.ToString() : MessageStatusType.Sent.ToString()
        });
    }
}
