using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SocialMedia.Api.Controllers;
using SocialMedia.Application.Auth.Commands.ChangePassword;
using SocialMedia.Application.Auth.Commands.ForgetPassword;
using SocialMedia.Application.Auth.Commands.Login;
using SocialMedia.Application.Auth.Commands.RefreshToken;
using SocialMedia.Application.Auth.Commands.Register;
using SocialMedia.Application.Auth.Commands.ResetPassword;
using SocialMedia.Application.Auth.Commands.SendEmailVerification;
using SocialMedia.Application.Auth.Commands.VerifyEmail;
using SocialMedia.Application.Auth.Queries.VerifyPasswordResetToken;
using SocialMedia.Application.Auth.Responses;
using SocialMedia.WebApi.Controllers.Auth.Requests;

namespace SocialMedia.WebApi.Controllers.Auth;

[Route("api/[controller]")]
public class AuthController(ISender sender) : ApiController
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticatedUserResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await sender.Send(new LoginCommand(request.Email, request.Password));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthenticatedUserResponse>> Register([FromForm] RegisterRequest request)
    {
        var result = await sender.Send(new RegisterCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.Avatar));

        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthenticatedUserResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await sender.Send(new RefreshTokenCommand(request.Token, request.RefreshToken));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendEmailVerification([FromBody] ResendEmailVerificationRequest request)
    {
        var result = await sender.Send(new SendEmailVerificationCommand(request.Email));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }

    [HttpGet("activate")]
    public async Task<ActionResult<bool>> VerifyEmail([FromQuery] string token)
    {
        var result = await sender.Send(new VerifyEmailCommand(token));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }

    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
    {
        var result = await sender.Send(new ForgetPasswordCommand(request.Email));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }

    [HttpGet("reset-password")]
    public async Task<ActionResult<bool>> VerifyPasswordResetToken([FromQuery] string token)
    {
        var result = await sender.Send(new VerifyPasswordResetTokenQuery(token));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await sender.Send(new ResetPasswordCommand(request.Token, request.NewPassword));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await sender.Send(new ChangePasswordCommand(request.OldPassword, request.NewPassword));
        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }
}