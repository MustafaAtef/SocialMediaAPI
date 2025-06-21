using System;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IAuthService
{
    Task<AuthenticatedUserDto> LoginAsync(LoginRequestDto loginRequest);
    Task<AuthenticatedUserDto> RegisterAsync(RegisterRequestDto registerRequest);
    Task<AuthenticatedUserDto> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenRequest);
    Task SendEmailVerificationAsync(EmailVerificationRequestDto emailVerificationRequest);
    Task<bool> VerifyEmailAsync(string token);
    Task ForgetPasswordAsync(ForgetPasswordRequestDto forgetPasswordRequestDto);
    Task<bool> IsPasswordResetTokenValidAsync(string token);
    Task ResetPasswordAsync(ResetPasswordRequestDto resetPasswordRequestDto);
    Task ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequestDto);
}
