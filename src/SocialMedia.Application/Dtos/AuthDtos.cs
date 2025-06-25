using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SocialMedia.Application.Dtos;

public class AuthenticatedUserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string AvatarUrl { get; set; }
    public bool IsEmailVerified { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public string Token { get; set; }
    public DateTime TokenExpirationDate { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpirationDate { get; set; }
}


public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }
}

public class RegisterRequestDto
{
    [Required(ErrorMessage = "First name is required.")]
    [Length(3, 50, ErrorMessage = "First name length must be between 3 and 50 characters.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [Length(3, 50, ErrorMessage = "Last name length must be between 3 and 50 characters.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [Length(10, 100, ErrorMessage = "Password length must be between 10 and 100 characters.")]
    public string Password { get; set; }
    public IFormFile? Avatar { get; set; }
}

public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Token is required.")]
    public string Token { get; set; }
    [Required(ErrorMessage = "Refresh token is required.")]
    public string RefreshToken { get; set; }
}

public class EmailVerificationRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    public string Email { get; set; }
}

public class ForgetPasswordRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    public string Email { get; set; }
}

public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "Token is required.")]
    public string Token { get; set; }
    [Required(ErrorMessage = "New Password is required.")]
    public string NewPassword { get; set; }
}

public class ChangePasswordRequestDto
{
    [Required(ErrorMessage = "Old password is required.")]
    public string OldPassword { get; set; }
    [Required(ErrorMessage = "New password is required.")]
    public string NewPassword { get; set; }
}
