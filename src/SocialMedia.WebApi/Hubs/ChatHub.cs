using System;
using Microsoft.AspNetCore.SignalR;
using SocialMedia.Application.Dtos;

namespace SocialMedia.WebApi.Hubs;

public class ChatHub : Hub
{
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();

    }
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    public void SendDirectMessage(DirectMessageDto directMessageDto)
    {
    }
    public void SendFirstDirectMessage(FirstDirectMessageDto firstDirectMessageDto)
    {

    }
}
