using System;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IPostService
{
    Task<PostDto> CreateAsync(CreatePostDto createPostDto);
    Task<PostDto> UpdateAsync(UpdatePostDto updatePostDto);
    Task<PagedList<UserPostsDto>> GetPagedPostsAsync(int userId, int page, int pageSize);
    Task<PostDto> GetPostAsync(int postId, int commentPageSize, int commentRepliesSize);
}
