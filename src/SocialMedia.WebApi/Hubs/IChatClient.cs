using SocialMedia.Application.Dtos;

namespace SocialMedia.WebApi.Hubs;

public interface IChatClient
{
    Task DeliveredMessages(DeliveredMessagesDto deliveredMessagesDto);
    Task SeenMessages(ReadMessagesInGroupDto readMessagesInGroupDto);
    Task NewMessage(MessageDto messageDto);
}
