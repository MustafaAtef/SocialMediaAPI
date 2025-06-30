using System;
using System.Security.Claims;
using EducationCenter.Core.RepositoryContracts;
using Microsoft.AspNetCore.Http;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Exceptions;

namespace SocialMedia.Application.Services;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    public UserService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    public async Task FollowAsync(int userId)
    {
        var tokenUser = GetAuthenticatedUser();
        if (tokenUser is null)
        {
            throw new UnAuthenticatedException("User is not authenticated.");
        }
        if (userId == tokenUser.Id) // to prevent self-follow
        {
            throw new BadRequestException("You cannot follow yourself.");
        }
        var followingUser = await _unitOfWork.Users.GetAsync(u => u.Id == userId);
        var followerUser = await _unitOfWork.Users.GetAsync(u => u.Id == tokenUser.Id);
        if (followingUser is null || followerUser is null)
        {
            throw new BadRequestException("Following user not found.");
        }
        var existingFollow = await _unitOfWork.FollowersFollowings.GetAsync(ff => ff.FollowerId == followerUser.Id && ff.FollowingId == followingUser.Id);
        if (existingFollow != null)
        {
            throw new BadRequestException("You already follow this user.");
        }
        _unitOfWork.FollowersFollowings.Add(new FollowerFollowing
        {
            FollowerId = followerUser.Id,
            FollowingId = followingUser.Id
        });
        followerUser.FollowingCount++;
        followingUser.FollowersCount++;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnFollowAsync(int userId)
    {
        var tokenUser = GetAuthenticatedUser();
        if (tokenUser is null)
        {
            throw new UnAuthenticatedException("User is not authenticated.");
        }
        if (userId == tokenUser.Id) // to prevent self-follow
        {
            throw new BadRequestException("Users cannot unfollow themselves.");
        }
        var followingUser = await _unitOfWork.Users.GetAsync(u => u.Id == userId);
        var followerUser = await _unitOfWork.Users.GetAsync(u => u.Id == tokenUser.Id);
        if (followingUser is null || followerUser is null)
        {
            throw new BadRequestException("Following user not found.");
        }
        var existingFollow = await _unitOfWork.FollowersFollowings.GetAsync(ff => ff.FollowerId == followerUser.Id && ff.FollowingId == followingUser.Id);
        if (existingFollow is null)
        {
            throw new BadRequestException("You are not following this user.");
        }
        _unitOfWork.FollowersFollowings.Remove(existingFollow);
        followerUser.FollowingCount--;
        followingUser.FollowersCount--;
        await _unitOfWork.SaveChangesAsync();
    }

    public UserDto? GetAuthenticatedUser()
    {
        var principal = _httpContextAccessor.HttpContext.User;
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

    public async Task<PagedList<UserDto>> GetPagedFollowersAsync(int userId, int page, int pageSize)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId);
        if (user is null)
        {
            throw new BadRequestException("User not found.");
        }

        var followers = await _unitOfWork.FollowersFollowings.GetAllAsync(page, pageSize, ff => ff.FollowingId == userId, null, null, ["Follower.Avatar"]);

        var count = await _unitOfWork.FollowersFollowings.CountAsync(ff => ff.FollowingId == userId);
        return new PagedList<UserDto>(count, pageSize, page, followers.Select(f => new UserDto()
        {
            Id = f.FollowerId,
            Name = f.Follower.FirstName + " " + f.Follower.LastName,
            Email = f.Follower.Email,
            AvatarUrl = f.Follower.Avatar?.Url ?? ""
        }).ToList());
    }

    public async Task<PagedList<UserDto>> GetPagedFollowingsAsync(int userId, int page, int pageSize)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId);
        if (user is null)
        {
            throw new BadRequestException("User not found.");
        }

        var followings = await _unitOfWork.FollowersFollowings.GetAllAsync(page, pageSize, ff => ff.FollowerId == userId, null, null, ["Following.Avatar"]);

        var count = await _unitOfWork.FollowersFollowings.CountAsync(ff => ff.FollowerId == userId);
        return new PagedList<UserDto>(count, pageSize, page, followings.Select(f => new UserDto()
        {
            Id = f.FollowerId,
            Name = f.Following.FirstName + " " + f.Following.LastName,
            Email = f.Following.Email,
            AvatarUrl = f.Following.Avatar?.Url ?? ""
        }).ToList());
    }
}
