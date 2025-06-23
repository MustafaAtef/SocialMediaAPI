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
    public string Email { get; set; }
    public string Password { get; set; }
}

public class RegisterRequestDto
{
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }
    public IFormFile? Avatar { get; set; }
}

public class RefreshTokenRequestDto
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}

public class EmailVerificationRequestDto
{
    public string Email { get; set; }
}

public class ForgetPasswordRequestDto
{
    public string Email { get; set; }
}

public class ResetPasswordRequestDto
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
}

public class ChangePasswordRequestDto
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}
