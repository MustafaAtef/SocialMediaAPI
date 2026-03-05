using Microsoft.AspNetCore.Http;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.Users.Commands.Update;

public record UpdateUserCommand(string? FirstName, string? LastName, IFormFile? Avatar) : ICommand<UserDto>;
