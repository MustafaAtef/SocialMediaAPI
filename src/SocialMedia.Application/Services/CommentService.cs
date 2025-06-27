using System;
using EducationCenter.Core.RepositoryContracts;
using Microsoft.AspNetCore.Http;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;

namespace SocialMedia.Application.Service;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CommentService(IUnitOfWork unitOfWork, IUserService userService, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<CommentWithoutRepliesDto> CreateAsync(CreateCommentDto createCommentDto)
    {
        var user = _userService.GetAuthenticatedUser();
        if (user == null)
        {
            throw new Exception("User is not authenticated.");
        }
        var post = await _unitOfWork.Posts.GetAsync(p => p.Id == createCommentDto.PostId);
        if (post == null)
        {
            throw new Exception("Post not found.");
        }
        var comment = new Comment
        {
            Content = createCommentDto.Content,
            UserId = user.Id,
            PostId = createCommentDto.PostId,
            ReactionsCount = 0,
            RepliesCount = 0
        };
        post.CommentsCount++;
        _unitOfWork.Comments.Add(comment);
        await _unitOfWork.SaveChangesAsync();
        return new CommentWithoutRepliesDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            CreatedBy = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            Content = comment.Content,
            ReactsCount = 0,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }

    public async Task<CommentWithoutRepliesDto> ReplyAsync(ReplyCommentDto replyCommentDto)
    {
        // check if the post exists then check if the parent comment exists
        var user = _userService.GetAuthenticatedUser();
        if (user == null)
        {
            throw new Exception("User is not authenticated.");
        }
        var post = await _unitOfWork.Posts.GetAsync(p => p.Id == replyCommentDto.PostId);
        if (post == null)
        {
            throw new Exception("Post not found.");
        }
        var parentComment = await _unitOfWork.Comments.GetAsync(c => c.Id == replyCommentDto.ParentCommentId && c.PostId == replyCommentDto.PostId);
        if (parentComment == null)
        {
            throw new Exception("Parent comment not found.");
        }
        if (parentComment.ParentComment is not null) // this ensures one level of replies
        {
            throw new Exception();
        }
        var reply = new Comment
        {
            Content = replyCommentDto.Content,
            UserId = user.Id,
            PostId = replyCommentDto.PostId,
            ParentCommentId = replyCommentDto.ParentCommentId,
            ReactionsCount = 0,
            RepliesCount = 0
        };
        parentComment.RepliesCount++;
        post.CommentsCount++;
        _unitOfWork.Comments.Add(reply);
        await _unitOfWork.SaveChangesAsync();
        return new CommentWithoutRepliesDto
        {
            Id = reply.Id,
            ParentCommentId = parentComment.Id,
            PostId = reply.PostId,
            CreatedBy = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            Content = reply.Content,
            ReactsCount = 0,
            CreatedAt = reply.CreatedAt,
            UpdatedAt = reply.UpdatedAt
        };
    }

    public async Task<CommentDto> UpdateAsync(UpdateCommentDto updateCommentDto)
    {
        // check if the post exists and the comment exists and the owner of the comment is the authenticated user then update the comment
        var user = _userService.GetAuthenticatedUser();
        if (user == null)
        {
            throw new Exception("User is not authenticated.");
        }
        var comment = await _unitOfWork.Comments.GetAsync(c => c.Id == updateCommentDto.CommentId && c.PostId == updateCommentDto.PostId);
        if (comment == null)
        {
            throw new Exception("Comment not found.");
        }
        if (comment.UserId != user.Id)
        {
            throw new Exception("unauthorized to update this comment.");
        }
        comment.Content = updateCommentDto.Content;
        comment.UpdatedAt = DateTime.Now;
        await _unitOfWork.SaveChangesAsync();
        return new CommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            Content = comment.Content,
            CreatedBy = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            ReactsCount = comment.ReactionsCount,
            RepliesCount = comment.RepliesCount,
            // LOAD REPLIES------------------------------------------------------------------------
            //Replies = new PagedList<CommentWithoutRepliesDto>(),
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
    public async Task<PagedList<CommentDto>> GetPagedCommentsAsync(int postId, int page, int pageSize, int repliesSize)
    {
        var post = await _unitOfWork.Posts.GetAsync(p => p.Id == postId);
        if (post is null)
        {
            throw new Exception();
        }
        var comments = await _unitOfWork.Comments.GetAllAsync(page, pageSize, repliesSize, postId);
        var commentsCount = await _unitOfWork.Comments.CountAsync(c => c.PostId == postId && c.ParentComment == null);
        return new PagedList<CommentDto>(commentsCount, pageSize, page, comments.Select(c => new CommentDto()
        {
            Id = c.Id,
            Content = c.Content,
            PostId = c.PostId,
            CreatedBy = new UserDto()
            {
                Id = c.User.Id,
                Name = c.User.FirstName + " " + c.User.LastName,
                Email = c.User.Email,
                AvatarUrl = c.User.Avatar?.Url ?? ""
            },
            ReactsCount = c.ReactionsCount,
            RepliesCount = c.RepliesCount,
            Replies = c.RepliesCount > 0 ? new PagedList<CommentWithoutRepliesDto>(c.RepliesCount, repliesSize, 1,
             c.Replies.Select(r => new CommentWithoutRepliesDto()
             {
                 Id = r.Id,
                 ParentCommentId = r.ParentCommentId,
                 PostId = r.PostId,
                 Content = r.Content,
                 CreatedBy = new UserDto()
                 {
                     Id = r.User.Id,
                     Name = r.User.FirstName + " " + r.User.LastName,
                     Email = r.User.Email,
                     AvatarUrl = r.User.Avatar?.Url ?? ""
                 },
                 ReactsCount = r.ReactionsCount,
                 CreatedAt = r.CreatedAt,
                 UpdatedAt = r.UpdatedAt
             }).ToList()) : null,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }));
    }

    public async Task<PagedList<CommentWithoutRepliesDto>> GetPagedRepliesAsync(int parentCommentId, int page, int pageSize)
    {
        var parentComment = await _unitOfWork.Comments.GetParentCommentWithReplies(parentCommentId, page, pageSize);
        if (parentComment is null)
        {
            throw new Exception();
        }
        return new PagedList<CommentWithoutRepliesDto>(parentComment.RepliesCount, pageSize, page, parentComment.Replies.Select(r => new CommentWithoutRepliesDto()
        {
            Id = r.Id,
            ParentCommentId = parentCommentId,
            PostId = r.PostId,
            Content = r.Content,
            CreatedBy = new UserDto
            {
                Id = r.User.Id,
                Name = r.User.FirstName + " " + r.User.LastName,
                Email = r.User.Email,
                AvatarUrl = r.User.Avatar?.Url ?? ""
            },
            ReactsCount = r.ReactionsCount,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList());
    }
}
