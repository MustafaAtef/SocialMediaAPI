using System;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IPostService
{
    Task<PostDto> CreateAsync(CreatePostDto createPostDto);
    Task<PostDto> UpdateAsync(UpdatePostDto updatePostDto);
    Task DeleteAsync(int postId);
    Task<PagedList<UserPostsDto>> GetPagedPostsAsync(int userId, int page, int pageSize);
    Task<PagedList<UserPostsDto>> GetPagedDeletedPostsAsync(int page, int pageSize);
    Task<PostDto> GetPostAsync(int postId, int commentPageSize, int commentRepliesSize);
    Task<UserPostsDto> RestoreDeletedPostAsync(int postId);
    Task PermanentDeleteAsync(int postId);
}
