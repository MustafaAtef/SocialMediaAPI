using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.RepositoryContracts;

namespace SocialMedia.Application.Users.Queries.GetPagedGroupMessages;

public sealed class GetPagedGroupMessagesQueryHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<GetPagedGroupMessagesQuery, GroupMessagesDto>
{
    public async Task<Result<GroupMessagesDto>> Handle(
        GetPagedGroupMessagesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.LastMessageId is null)
        {
            return Result.Failure<GroupMessagesDto>(
                new Error(
                    ErrorType.Validation,
                    "Messages.LastMessageIdRequired",
                    "LastMessageId is required."));
        }

        var group = await unitOfWork.Groups.GetAsync(g => g.Id == request.GroupId);
        if (group is null)
        {
            return Result.Failure<GroupMessagesDto>(
                new Error(
                    ErrorType.NotFound,
                    "Group.NotFound",
                    "The specified group was not found."));
        }

        var user = await unitOfWork.Users.GetPagedGroupMessagesAsync(
            request.UserId,
            request.GroupId,
            request.LastMessageId.Value,
            request.OlderMessagesSize);

        if (user is null)
            return Result.Failure<GroupMessagesDto>(UserErrors.NotFound);

        var allMembers = new Dictionary<int, User>();
        foreach (var groupMembers in user.Groups)
        {
            foreach (var member in groupMembers.Users)
            {
                if (!allMembers.ContainsKey(member.Id))
                    allMembers.Add(member.Id, member);
            }
        }

        var allOlderMessagesCount = await unitOfWork.Messages.CountAsync(
            m => m.GroupId == request.GroupId && m.Id < request.LastMessageId.Value);

        var response = user.Groups.Select(g => new GroupMessagesDto
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
                HasOlderMessages = allOlderMessagesCount > request.OlderMessagesSize,
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
        }).FirstOrDefault();

        if (response is null)
        {
            return Result.Failure<GroupMessagesDto>(
                new Error(
                    ErrorType.NotFound,
                    "Messages.GroupDataNotFound",
                    "No messages were found for the specified group."));
        }

        return Result.Success(response);
    }
}