using System;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IReactService
{
    Task<PostReactDto> ReactToPostAsync(ReactToPostDto reactToPostDto);
    Task<CommentReactDto> ReactToCommentAsync(ReactToCommentDto reactToCommentDto);
    Task RemovePostReactAsync(int postId);
    Task RemoveCommentReactAsync(int commentId);
    Task<PagedList<PostReactDto>> GetPagedPostReactsAsync(int postId, int page, int pageSize);
    Task<PagedList<CommentReactDto>> GetPagedcommentReactsAsync(int commentId, int page, int pageSize);

}
