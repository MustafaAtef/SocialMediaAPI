using System.Security.Claims;
using SocialMedia.Application.Common;
using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Configuration;
using SocialMedia.Core.Exceptions;

namespace SocialMedia.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IFileUploader _fileUploader;

    private readonly IEmailOutboxWriter _emailOutboxWriter;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IFileUploader fileUploader, IEmailOutboxWriter emailOutboxWriter)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _fileUploader = fileUploader;
        _emailOutboxWriter = emailOutboxWriter;
    }

    public async Task<AuthenticatedUserDto> RegisterAsync(RegisterRequestDto registerRequest)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Email == registerRequest.Email);
        if (user != null)
        {
            throw new UniqueException("User already exists with this email.");
        }
        var newUser = new User
        {
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName,
            Email = registerRequest.Email,
            Password = _passwordHasher.HashPassword(registerRequest.Password),
            IsEmailVerified = false,
        };

        JwtDto jwtData = _jwtService.GenerateToken(newUser);
        newUser.RefreshToken = jwtData.RefreshToken;
        newUser.RefreshTokenExpiryTime = jwtData.RefreshTokenExpirationDate;
        newUser.EmailVerificationToken = CryptoHelper.GenerateRandomToken();
        newUser.EmailVerificationTokenExpiryTime = DateTime.Now.AddMinutes(_configuration["EmailVerificationTokenExpiryMinutes"] != null ? int.Parse(_configuration["EmailVerificationTokenExpiryMinutes"] ?? "") : 15);
        if (registerRequest.Avatar != null)
        {
            var uploadedImage = await _fileUploader.UploadAsync(registerRequest.Avatar, "users-avatars");
            newUser.Avatar = new Avatar
            {
                StorageProvider = uploadedImage.StorageProvider,
                Url = uploadedImage.Url
            };
        }
        _unitOfWork.Users.Add(newUser);
        _emailOutboxWriter.QueueVerificationEmail(
            newUser.Email,
            newUser.EmailVerificationToken!,
            newUser.EmailVerificationTokenExpiryTime!.Value);
        await _unitOfWork.SaveChangesAsync();

        return new AuthenticatedUserDto
        {
            Id = newUser.Id,
            Name = newUser.FirstName + " " + newUser.LastName,
            Email = newUser.Email,
            IsEmailVerified = false,
            FollowersCount = newUser.FollowersCount,
            FollowingCount = newUser.FollowingCount,
            Token = jwtData.Token,
            TokenExpirationDate = jwtData.TokenExpirationDate,
            RefreshToken = jwtData.RefreshToken,
            RefreshTokenExpirationDate = jwtData.RefreshTokenExpirationDate,
            AvatarUrl = newUser.Avatar is null ? "" : newUser.Avatar.Url
        };
    }

    public async Task<AuthenticatedUserDto> LoginAsync(LoginRequestDto loginRequest)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Email == loginRequest.Email, ["Avatar"]);
        if (user == null || !_passwordHasher.VerifyPassword(loginRequest.Password, user.Password))
        {

            throw new BadRequestException("Invalid email or password.");
        }

        JwtDto jwtData = _jwtService.GenerateToken(user);
        user.RefreshToken = jwtData.RefreshToken;
        user.RefreshTokenExpiryTime = jwtData.RefreshTokenExpirationDate;

        await _unitOfWork.SaveChangesAsync();
        return new AuthenticatedUserDto
        {
            Id = user.Id,
            Name = user.FirstName + " " + user.LastName,
            Email = user.Email,
            IsEmailVerified = user.IsEmailVerified,
            FollowersCount = user.FollowersCount,
            FollowingCount = user.FollowingCount,
            Token = jwtData.Token,
            TokenExpirationDate = jwtData.TokenExpirationDate,
            RefreshToken = jwtData.RefreshToken,
            RefreshTokenExpirationDate = jwtData.RefreshTokenExpirationDate,
            AvatarUrl = user.Avatar?.Url ?? ""
        };
    }

    public async Task<AuthenticatedUserDto> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenRequest)
    {
        var userClaims = _jwtService.ValidateJwt(refreshTokenRequest.Token);
        if (userClaims == null)
        {
            throw new BadRequestException("Invalid refresh token.");
        }
        var userId = int.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "-1");
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId, ["Avatar"]);
        if (user == null || user.RefreshToken != refreshTokenRequest.RefreshToken || user.RefreshTokenExpiryTime < DateTime.Now)
        {
            throw new BadRequestException("Invalid refresh token.");
        }
        JwtDto jwtData = _jwtService.GenerateToken(user);
        user.RefreshToken = jwtData.RefreshToken;
        user.RefreshTokenExpiryTime = jwtData.RefreshTokenExpirationDate;
        await _unitOfWork.SaveChangesAsync();
        return new AuthenticatedUserDto
        {
            Id = user.Id,
            Name = user.FirstName + " " + user.LastName,
            Email = user.Email,
            IsEmailVerified = user.IsEmailVerified,
            FollowersCount = user.FollowersCount,
            FollowingCount = user.FollowingCount,
            Token = jwtData.Token,
            TokenExpirationDate = jwtData.TokenExpirationDate,
            RefreshToken = jwtData.RefreshToken,
            RefreshTokenExpirationDate = jwtData.RefreshTokenExpirationDate,
            AvatarUrl = user.Avatar?.Url ?? ""
        };
    }
    public async Task ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequestDto)
    {
        var userId = int.Parse(_httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        if (userId <= 0)
        {
            throw new UnAuthenticatedException("User not authenticated.");
        }
        var user = await _unitOfWork.Users.GetAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new BadRequestException("User not found.");
        }
        if (_passwordHasher.VerifyPassword(changePasswordRequestDto.OldPassword, user.Password) == false)
        {
            throw new BadRequestException("Old password is incorrect.");
        }
        user.Password = _passwordHasher.HashPassword(changePasswordRequestDto.NewPassword);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ForgetPasswordAsync(ForgetPasswordRequestDto forgetPasswordRequestDto)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Email == forgetPasswordRequestDto.Email);
        if (user == null)
        {
            throw new BadRequestException("User not found.");
        }

        var token = CryptoHelper.GenerateRandomToken();
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiryTime = DateTime.Now.AddMinutes(_configuration["PasswordResetTokenExpiryMinutes"] != null ? int.Parse(_configuration["PasswordResetTokenExpiryMinutes"] ?? "") : 15);

        _emailOutboxWriter.QueuePasswordResetEmail(
            user.Email,
            user.PasswordResetToken!,
            user.PasswordResetTokenExpiryTime!.Value);
        await _unitOfWork.SaveChangesAsync();
    }
    public async Task<bool> IsPasswordResetTokenValidAsync(string token)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.PasswordResetToken == token);
        if (user == null || user.PasswordResetTokenExpiryTime < DateTime.Now)
        {
            return false;
        }
        return true;
    }
    public async Task ResetPasswordAsync(ResetPasswordRequestDto resetPasswordRequestDto)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.PasswordResetToken == resetPasswordRequestDto.Token);
        if (user == null || user.PasswordResetTokenExpiryTime < DateTime.Now)
        {
            throw new BadRequestException("Invalid or expired password reset token.");
        }
        user.Password = _passwordHasher.HashPassword(resetPasswordRequestDto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiryTime = null;
        await _unitOfWork.SaveChangesAsync();
    }


    public async Task SendEmailVerificationAsync(EmailVerificationRequestDto emailVerificationRequest)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.Email == emailVerificationRequest.Email);
        if (user == null)
        {
            throw new BadRequestException("User not found.");
        }
        if (user.IsEmailVerified)
        {
            throw new BadRequestException("Email is already verified.");
        }

        user.EmailVerificationToken = CryptoHelper.GenerateRandomToken();
        user.EmailVerificationTokenExpiryTime = DateTime.Now.AddMinutes(_configuration["EmailVerificationTokenExpiryMinutes"] != null ? int.Parse(_configuration["EmailVerificationTokenExpiryMinutes"] ?? "") : 15);

        _emailOutboxWriter.QueueVerificationEmail(
            user.Email,
            user.EmailVerificationToken!,
            user.EmailVerificationTokenExpiryTime!.Value);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _unitOfWork.Users.GetAsync(u => u.EmailVerificationToken == token);
        if (user == null || user.EmailVerificationTokenExpiryTime < DateTime.Now)
        {
            return false;
        }
        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiryTime = null;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }


}
