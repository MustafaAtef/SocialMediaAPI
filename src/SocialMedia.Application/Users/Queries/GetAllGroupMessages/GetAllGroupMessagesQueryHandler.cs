using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.RepositoryContracts;

namespace SocialMedia.Application.Users.Queries.GetAllGroupMessages;

public sealed class GetAllGroupMessagesQueryHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<GetAllGroupMessagesQuery, ICollection<GroupMessagesDto>>
{
    public async Task<Result<ICollection<GroupMessagesDto>>> Handle(
        GetAllGroupMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetAllGroupsMessagesAsync(request.UserId, request.OlderMessagesSize);
        if (user is null)
            return Result.Failure<ICollection<GroupMessagesDto>>(UserErrors.NotFound);

        var allMembers = new Dictionary<int, User>();
        foreach (var groupMembers in user.Groups)
        {
            foreach (var member in groupMembers.Users)
            {
                if (!allMembers.ContainsKey(member.Id))
                    allMembers.Add(member.Id, member);
            }
        }

        var result = user.Groups.Select(g => new GroupMessagesDto
        {
            GroupId = g.Id,
            Name = g.Name,
            Type = g.Type.ToString(),
            Members = g.Users.Select(gu => new UserDto
            {
                Id = gu.Id,
                Name = gu.FirstName + " " + gu.LastName,
                Email = gu.Email,
                AvatarUrl = gu.Avatar?.Url ?? ""
            }).ToList(),
            Messages = new PagedMessagesDto
            {
                PageSize = request.OlderMessagesSize,
                LastMessageId = g.Messages.Count > 0 ? g.Messages.Last().Id : -1,
                HasOlderMessages = g.TotalMessages > request.OlderMessagesSize,
                GroupId = g.Id,
                Data = g.Messages.Select(gm => new MessageDto
                {
                    Id = gm.Id,
                    GroupId = g.Id,
                    Message = gm.Data,
                    SentBy = new UserDto
                    {
                        Id = gm.FromId,
                        Name = allMembers[gm.FromId].FirstName + " " + allMembers[gm.FromId].LastName,
                        Email = allMembers[gm.FromId].Email,
                        AvatarUrl = allMembers[gm.FromId].Avatar?.Url ?? ""
                    },
                    CreatedAt = gm.CreatedAt,
                    Status = gm.MessageStatuses.Select(ms => new MessageStatusDto
                    {
                        RecievedBy = new UserDto
                        {
                            Id = ms.RecieverId,
                            Name = allMembers[ms.RecieverId].FirstName + " " + allMembers[ms.RecieverId].LastName,
                            Email = allMembers[ms.RecieverId].Email,
                            AvatarUrl = allMembers[ms.RecieverId].Avatar?.Url ?? ""
                        },
                        StatusType = ms.Status,
                        Status = ms.Status.ToString(),
                        SentAt = ms.SentAt,
                        DeliveredAt = ms.DeliveredAt,
                        SeenAt = ms.SeenAt
                    }).ToList()
                }).ToList()
            }
        }).ToList();

        return Result.Success<ICollection<GroupMessagesDto>>(result);
    }
}