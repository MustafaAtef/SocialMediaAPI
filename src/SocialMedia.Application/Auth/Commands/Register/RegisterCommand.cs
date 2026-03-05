using Microsoft.AspNetCore.Http;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.Auth.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    IFormFile? Avatar) : ICommand<AuthenticatedUserDto>;
