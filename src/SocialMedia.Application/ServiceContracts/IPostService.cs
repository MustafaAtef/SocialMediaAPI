using System;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IPostService
{
    Task<PostDto> CreateAsync(CreatePostDto createPostDto);
    Task<PostDto> UpdateAsync(UpdatePostDto updatePostDto);
}
