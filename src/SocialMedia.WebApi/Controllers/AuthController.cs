using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SocialMedia.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticatedUserDto>> Login(LoginRequestDto loginRequest)
        {
            return await _authService.LoginAsync(loginRequest);
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthenticatedUserDto>> Register([FromForm] RegisterRequestDto registerRequest)
        {
            return await _authService.RegisterAsync(registerRequest);
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthenticatedUserDto>> RefreshToken(RefreshTokenRequestDto refreshTokenRequest)
        {
            return await _authService.RefreshTokenAsync(refreshTokenRequest);
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendEmailVerification(EmailVerificationRequestDto emailVerificationRequest)
        {
            await _authService.SendEmailVerificationAsync(emailVerificationRequest);
            return Ok();
        }

        [HttpGet("activate")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var isVerified = await _authService.VerifyEmailAsync(token);
            if (isVerified)
            {
                return Ok();
            }
            return BadRequest("Invalid or expired verification token.");
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequestDto forgetPasswordRequestDto)
        {
            await _authService.ForgetPasswordAsync(forgetPasswordRequestDto);
            return Ok();
        }

        [HttpGet("reset-password")]
        public async Task<IActionResult> VerifyPasswordResetToken([FromQuery] string token)
        {
            var isValid = await _authService.IsPasswordResetTokenValidAsync(token);
            if (isValid)
            {
                return Ok();
            }
            return BadRequest("Invalid or expired password reset token.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto resetPasswordRequestDto)
        {
            await _authService.ResetPasswordAsync(resetPasswordRequestDto);
            return Ok();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto changePasswordRequestDto)
        {
            await _authService.ChangePasswordAsync(changePasswordRequestDto);
            return Ok();
        }

    }
}
