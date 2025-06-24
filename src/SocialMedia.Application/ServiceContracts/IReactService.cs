using System;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IReactService
{
    Task<PostReactDto> ReactToPostAsync(ReactToPostDto reactToPostDto);
    Task<CommentReactDto> ReactToCommentAsync(ReactToCommentDto reactToCommentDto);
    Task RemovePostReactAsync(int postId);
    Task RemoveCommentReactAsync(int commentId);
}
