using System;
using EducationCenter.Core.RepositoryContracts;
using SocialMedia.Application.Dtos;
using SocialMedia.Core.Entities;

namespace SocialMedia.Application.ServiceContracts;

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
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        // check if the post exists
        var post = await _unitOfWork.Posts.GetByIdAsync(reactToPostDto.PostId);
        if (post == null)
        {
            throw new ArgumentException("Post not found.");
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
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        var post = await _unitOfWork.Posts.GetByIdAsync(reactToCommentDto.PostId);
        if (post is null)
        {
            throw new ArgumentException("Post not found.");
        }
        var comment = await _unitOfWork.Comments.GetByIdAsync(reactToCommentDto.CommentId);
        if (comment is null)
        {
            throw new ArgumentException("Comment not found.");
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
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        var post = await _unitOfWork.Posts.GetByIdAsync(postId);
        if (post is null)
        {
            throw new ArgumentException("Post not found.");
        }
        var postReact = await _unitOfWork.PostReacts.GetAsync(pr => pr.PostId == postId && pr.UserId == user.Id);
        if (postReact is null)
        {
            throw new ArgumentException("React not found.");
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
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
        if (comment is null)
        {
            throw new ArgumentException("Comment not found.");
        }
        var commentReact = await _unitOfWork.CommentReacts.GetAsync(cr => cr.CommentId == commentId && cr.UserId == user.Id);
        if (commentReact is null)
        {
            throw new ArgumentException("React not found.");
        }
        _unitOfWork.CommentReacts.Remove(commentReact);
        comment.ReactionsCount--;
        await _unitOfWork.SaveChangesAsync();
    }


}
