using Microsoft.AspNetCore.Http;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Auth.Responses;

namespace SocialMedia.Application.Auth.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    IFormFile? Avatar) : ICommand<AuthenticatedUserResponse>;
