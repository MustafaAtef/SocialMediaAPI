using System;
using System.Security.Claims;
using SocialMedia.Core.RepositoryContracts;
using Microsoft.AspNetCore.Http;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Exceptions;

namespace SocialMedia.Application.Services;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileUploader _fileUploader;

    public UserService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, IFileUploader fileUploader)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _fileUploader = fileUploader;
    }


    public UserDto? GetAuthenticatedUser(ClaimsPrincipal? claimsPrincipal = null)
    {
        ClaimsPrincipal? principal;
        if (claimsPrincipal is null)
            principal = _httpContextAccessor.HttpContext.User;
        else principal = claimsPrincipal;
        var id = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = principal?.FindFirst(ClaimTypes.Email)?.Value;
        var name = principal?.FindFirst("name")?.Value;
        var avatarUrl = principal?.FindFirst("avatarUrl")?.Value;
        if (id == null)
        {
            return null;
        }
        return new UserDto
        {
            Id = int.Parse(id),
            Email = email,
            Name = name,
            AvatarUrl = avatarUrl ?? string.Empty
        };
    }

    public async Task<ICollection<GroupMessagesDto>> GetAllGroupMessagesAsync(int olderMessagesSize)
    {
        var tokenUser = GetAuthenticatedUser();
        if (tokenUser is null) throw new UnAuthenticatedException("User is not authenticated.");
        var user = await _unitOfWork.Users.GetAllGroupsMessagesAsync(tokenUser.Id, olderMessagesSize);
        var allMembers = new Dictionary<int, User>();
        foreach (var groupMembers in user.Groups)
        {
            foreach (var member in groupMembers.Users)
            {
                if (!allMembers.ContainsKey(member.Id))
                    allMembers.Add(member.Id, member);
            }
        }
        if (user is null) throw new BadRequestException("User not found.");
        return user.Groups.Select(g => new GroupMessagesDto()
        {
            GroupId = g.Id,
            Name = g.Name,
            Type = g.Type.ToString(),
            Members = g.Users.Select(gu => new UserDto()
            {
                Id = gu.Id,
                Name = gu.FirstName + " " + gu.LastName,
                Email = gu.Email,
                AvatarUrl = gu.Avatar?.Url ?? ""
            }).ToList(),
            Messages = new PagedMessagesDto()
            {
                PageSize = olderMessagesSize,
                LastMessageId = g.Messages.Last().Id,
                HasOlderMessages = g.TotalMessages > olderMessagesSize,
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
    }

    public async Task<GroupMessagesDto> GetPagedGroupMessagesAsync(Guid groupId, int? lastMessageId, int olderMessagesSize)
    {
        var tokenUser = GetAuthenticatedUser();
        if (tokenUser is null) throw new UnAuthenticatedException("User is not authenticated.");
        if (lastMessageId is null) throw new BadRequestException("LastMessageId is required.");
        var group = await _unitOfWork.Groups.GetAsync(g => g.Id == groupId);
        if (group is null) throw new BadRequestException("Group not found.");
        var user = await _unitOfWork.Users.GetPagedGroupMessagesAsync(tokenUser.Id, groupId, lastMessageId.Value, olderMessagesSize);
        var allMembers = new Dictionary<int, User>();
        foreach (var groupMembers in user.Groups)
        {
            foreach (var member in groupMembers.Users)
            {
                if (!allMembers.ContainsKey(member.Id))
                    allMembers.Add(member.Id, member);
            }
        }
        if (user is null) throw new BadRequestException("User not found.");
        var allOlderMessagesCount = await _unitOfWork.Messages.CountAsync(m => m.GroupId == groupId && m.Id < lastMessageId);
        return user.Groups.Select(g => new GroupMessagesDto()
        {
            GroupId = g.Id,
            Name = g.Name,
            Type = g.Type.ToString(),
            Members = g.Users.Select(gu => new UserDto()
            {
                Id = gu.Id,
                Name = gu.FirstName + " " + gu.LastName,
                Email = gu.Email,
                AvatarUrl = gu.Avatar?.Url ?? ""
            }).ToList(),
            Messages = new PagedMessagesDto()
            {
                PageSize = olderMessagesSize,
                LastMessageId = g.Messages.Count > 0 ? g.Messages.Last().Id : -1,
                HasOlderMessages = allOlderMessagesCount > olderMessagesSize,
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
        }).ToList()[0];
    }
}
