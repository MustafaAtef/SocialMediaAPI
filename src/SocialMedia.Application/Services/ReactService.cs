using System;
using EducationCenter.Core.RepositoryContracts;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Exceptions;

namespace SocialMedia.Application.Service;

public class ReactService : IReactService
{
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    public ReactService(IUserService userService, IUnitOfWork unitOfWork)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
    }
    public async Task<PostReactDto> ReactToPostAsync(ReactToPostDto reactToPostDto)
    {
        // get the authenticated user from the context
        var user = _userService.GetAuthenticatedUser();
        if (user == null)
        {
            throw new UnAuthenticatedException("User is not authenticated.");
        }
        // check if the post exists
        var post = await _unitOfWork.Posts.GetByIdAsync(reactToPostDto.PostId);
        if (post == null)
        {
            throw new BadRequestException("Post not found.");
        }
        // add the react to the post

        var postReact = await _unitOfWork.PostReacts.GetAsync(pr => pr.PostId == reactToPostDto.PostId && pr.UserId == user.Id);
        if (postReact != null)
        {
            // if the react already exists, update it
            postReact.ReactType = reactToPostDto.ReactType;
        }
        else
        {
            // if the react does not exist, create a new one
            postReact = new PostReact
            {
                PostId = reactToPostDto.PostId,
                UserId = user.Id,
                ReactType = reactToPostDto.ReactType,
                CreatedAt = DateTime.UtcNow
            };
            post.ReactionsCount++;
            _unitOfWork.PostReacts.Add(postReact);
        }

        await _unitOfWork.SaveChangesAsync();
        return new()
        {
            Id = postReact.Id,
            PostId = postReact.PostId,
            ReactedBy = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            ReactType = postReact.ReactType,
            Name = postReact.ReactType.ToString(),
            CreatedAt = postReact.CreatedAt
        };
    }
    public async Task<CommentReactDto> ReactToCommentAsync(ReactToCommentDto reactToCommentDto)
    {
        var user = _userService.GetAuthenticatedUser();
        if (user is null)
        {
            throw new UnAuthenticatedException("User is not authenticated.");
        }
        var post = await _unitOfWork.Posts.GetByIdAsync(reactToCommentDto.PostId);
        if (post is null)
        {
            throw new BadRequestException("Post not found.");
        }
        var comment = await _unitOfWork.Comments.GetByIdAsync(reactToCommentDto.CommentId);
        if (comment is null)
        {
            throw new BadRequestException("Comment not found.");
        }
        var commentReact = await _unitOfWork.CommentReacts.GetAsync(cr => cr.CommentId == reactToCommentDto.CommentId && cr.UserId == user.Id);
        if (commentReact != null)
        {
            // if the react already exists, update it
            commentReact.ReactType = reactToCommentDto.ReactType;
        }
        else
        {
            // if the react does not exist, create a new one
            commentReact = new CommentReact
            {
                CommentId = reactToCommentDto.CommentId,
                UserId = user.Id,
                ReactType = reactToCommentDto.ReactType,
                CreatedAt = DateTime.UtcNow
            };
            comment.ReactionsCount++;
            _unitOfWork.CommentReacts.Add(commentReact);
        }
        await _unitOfWork.SaveChangesAsync();
        return new CommentReactDto
        {
            Id = commentReact.Id,
            CommentId = commentReact.CommentId,
            ReactedBy = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            ReactType = commentReact.ReactType,
            Name = commentReact.ReactType.ToString(),
            CreatedAt = commentReact.CreatedAt
        };

    }
    public async Task RemovePostReactAsync(int postId)
    {
        var user = _userService.GetAuthenticatedUser();
        if (user is null)
        {
            throw new UnAuthenticatedException("User is not authenticated.");
        }
        var post = await _unitOfWork.Posts.GetByIdAsync(postId);
        if (post is null)
        {
            throw new BadRequestException("Post not found.");
        }
        var postReact = await _unitOfWork.PostReacts.GetAsync(pr => pr.PostId == postId);
        if (postReact is null)
        {
            throw new BadRequestException("React not found.");
        }
        if (postReact.UserId != user.Id)
        {
            throw new UnAuthorizedException("User is not authorized to remove this react.");
        }
        _unitOfWork.PostReacts.Remove(postReact);
        post.ReactionsCount--;
        await _unitOfWork.SaveChangesAsync();
    }
    public async Task RemoveCommentReactAsync(int commentId)
    {
        var user = _userService.GetAuthenticatedUser();
        if (user is null)
        {
            throw new UnAuthenticatedException("User is not authenticated.");
        }
        var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
        if (comment is null)
        {
            throw new BadRequestException("Comment not found.");
        }
        var commentReact = await _unitOfWork.CommentReacts.GetAsync(cr => cr.CommentId == commentId && cr.UserId == user.Id);
        if (commentReact is null)
        {
            throw new BadRequestException("React not found.");
        }
        _unitOfWork.CommentReacts.Remove(commentReact);
        comment.ReactionsCount--;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedList<PostReactDto>> GetPagedPostReactsAsync(int postId, int page, int pageSize)
    {
        var post = await _unitOfWork.Posts.GetAsync(p => p.Id == postId);
        if (post is null)
        {
            throw new BadRequestException("Post not found.");
        }
        var postReactions = await _unitOfWork.PostReacts.GetAllAsync(page, pageSize, pr => pr.PostId == postId, pr => pr.CreatedAt, "desc", ["User.Avatar"]);
        return new(post.ReactionsCount, pageSize, page, postReactions.Select(pr => new PostReactDto()
        {
            Id = pr.Id,
            PostId = pr.PostId,
            ReactedBy = new UserDto
            {
                Id = pr.User.Id,
                Name = pr.User.FirstName + " " + pr.User.LastName,
                Email = pr.User.Email,
                AvatarUrl = pr.User.Avatar?.Url ?? ""
            },
            ReactType = pr.ReactType,
            Name = pr.ReactType.ToString(),
            CreatedAt = pr.CreatedAt
        }).ToList());
    }

    public async Task<PagedList<CommentReactDto>> GetPagedcommentReactsAsync(int commentId, int page, int pageSize)
    {
        var comment = await _unitOfWork.Comments.GetAsync(c => c.Id == commentId);
        if (comment is null)
        {
            throw new BadRequestException("Comment not found.");
        }
        var commentReactions = await _unitOfWork.CommentReacts.GetAllAsync(
            page,
            pageSize,
            cr => cr.CommentId == commentId,
            cr => cr.CreatedAt,
            "desc",
            ["User.Avatar"]
        );
        return new PagedList<CommentReactDto>(
            comment.ReactionsCount,
            pageSize,
            page,
            commentReactions.Select(cr => new CommentReactDto
            {
                Id = cr.Id,
                CommentId = cr.CommentId,
                ReactedBy = new UserDto
                {
                    Id = cr.User.Id,
                    Name = cr.User.FirstName + " " + cr.User.LastName,
                    Email = cr.User.Email,
                    AvatarUrl = cr.User.Avatar?.Url ?? ""
                },
                ReactType = cr.ReactType,
                Name = cr.ReactType.ToString(),
                CreatedAt = cr.CreatedAt
            }).ToList()
        );
    }
}
