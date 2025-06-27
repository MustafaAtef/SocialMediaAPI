using System;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface ICommentService
{
    Task<CommentWithoutRepliesDto> CreateAsync(CreateCommentDto createCommentDto);
    Task<CommentWithoutRepliesDto> ReplyAsync(ReplyCommentDto replyCommentDto);
    Task<CommentDto> UpdateAsync(UpdateCommentDto updateCommentDto);
    Task<PagedList<CommentDto>> GetPagedCommentsAsync(int postId, int page, int pageSize, int repliesSize);
    Task<PagedList<CommentWithoutRepliesDto>> GetPagedRepliesAsync(int commentId, int page, int pageSize);
}
