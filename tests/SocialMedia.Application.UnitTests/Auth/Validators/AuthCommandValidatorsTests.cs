using FluentValidation.TestHelper;

using SocialMedia.Application.Auth.Commands.ChangePassword;
using SocialMedia.Application.Auth.Commands.ForgetPassword;
using SocialMedia.Application.Auth.Commands.Login;
using SocialMedia.Application.Auth.Commands.RefreshToken;
using SocialMedia.Application.Auth.Commands.Register;
using SocialMedia.Application.Auth.Commands.ResetPassword;
using SocialMedia.Application.Auth.Commands.SendEmailVerification;
using SocialMedia.Application.Auth.Commands.VerifyEmail;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class AuthCommandValidatorsTests
{
    [Fact]
    public void LoginValidator_WhenCommandInvalid_ShouldHaveErrorsForEmailAndPassword()
    {
        var sut = new LoginCommandValidator();

        var result = sut.TestValidate(new LoginCommand("invalid-email", string.Empty));

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void LoginValidator_WhenCommandValid_ShouldNotHaveErrors()
    {
        var sut = new LoginCommandValidator();

        var result = sut.TestValidate(new LoginCommand("user@example.com", "StrongPassword123"));

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterValidator_WhenCommandInvalid_ShouldHaveErrorsForRequiredFields()
    {
        var sut = new RegisterCommandValidator();

        var result = sut.TestValidate(new RegisterCommand("Mo", "At", "bad", "short", null));

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterValidator_WhenCommandValid_ShouldNotHaveErrors()
    {
        var sut = new RegisterCommandValidator();

        var result = sut.TestValidate(new RegisterCommand("Mostafa", "Atef", "user@example.com", "StrongPassword123", null));

        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ChangePasswordValidator_WhenCommandInvalid_ShouldHaveErrorForNewPassword()
    {
        var sut = new ChangePasswordCommandValidator();

        var result = sut.TestValidate(new ChangePasswordCommand("old-password", "old-password"));

        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ChangePasswordValidator_WhenCommandValid_ShouldNotHaveErrors()
    {
        var sut = new ChangePasswordCommandValidator();

        var result = sut.TestValidate(new ChangePasswordCommand("OldPassword123", "NewPassword123"));

        result.ShouldNotHaveValidationErrorFor(x => x.OldPassword);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ForgetPasswordValidator_WhenCommandInvalid_ShouldHaveErrorForEmail()
    {
        var sut = new ForgetPasswordCommandValidator();

        var result = sut.TestValidate(new ForgetPasswordCommand("invalid"));

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ForgetPasswordValidator_WhenCommandValid_ShouldNotHaveErrorForEmail()
    {
        var sut = new ForgetPasswordCommandValidator();

        var result = sut.TestValidate(new ForgetPasswordCommand("user@example.com"));

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void RefreshTokenValidator_WhenCommandInvalid_ShouldHaveErrorsForTokenAndRefreshToken()
    {
        var sut = new RefreshTokenCommandValidator();

        var result = sut.TestValidate(new RefreshTokenCommand(string.Empty, string.Empty));

        result.ShouldHaveValidationErrorFor(x => x.Token);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Fact]
    public void RefreshTokenValidator_WhenCommandValid_ShouldNotHaveErrors()
    {
        var sut = new RefreshTokenCommandValidator();

        var result = sut.TestValidate(new RefreshTokenCommand("jwt-token", "refresh-token"));

        result.ShouldNotHaveValidationErrorFor(x => x.Token);
        result.ShouldNotHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Fact]
    public void ResetPasswordValidator_WhenCommandInvalid_ShouldHaveErrorsForTokenAndNewPassword()
    {
        var sut = new ResetPasswordCommandValidator();

        var result = sut.TestValidate(new ResetPasswordCommand(string.Empty, "short"));

        result.ShouldHaveValidationErrorFor(x => x.Token);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ResetPasswordValidator_WhenCommandValid_ShouldNotHaveErrors()
    {
        var sut = new ResetPasswordCommandValidator();

        var result = sut.TestValidate(new ResetPasswordCommand("reset-token", "VeryStrongPassword123"));

        result.ShouldNotHaveValidationErrorFor(x => x.Token);
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void SendEmailVerificationValidator_WhenCommandInvalid_ShouldHaveErrorForEmail()
    {
        var sut = new SendEmailVerificationCommandValidator();

        var result = sut.TestValidate(new SendEmailVerificationCommand("invalid"));

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void SendEmailVerificationValidator_WhenCommandValid_ShouldNotHaveErrorForEmail()
    {
        var sut = new SendEmailVerificationCommandValidator();

        var result = sut.TestValidate(new SendEmailVerificationCommand("user@example.com"));

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void VerifyEmailValidator_WhenCommandInvalid_ShouldHaveErrorForToken()
    {
        var sut = new VerifyEmailCommandValidator();

        var result = sut.TestValidate(new VerifyEmailCommand(string.Empty));

        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void VerifyEmailValidator_WhenCommandValid_ShouldNotHaveErrorForToken()
    {
        var sut = new VerifyEmailCommandValidator();

        var result = sut.TestValidate(new VerifyEmailCommand("verification-token"));

        result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
}
