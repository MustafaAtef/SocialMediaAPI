using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Auth.Commands.VerifyEmail;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

[Trait("Category", "Unit")]
public class VerifyEmailCommandHandlerTests
{
    private readonly VerifyEmailCommandHandler _sut;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;

    public VerifyEmailCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _users = Substitute.For<IUserRepository>();

        _unitOfWork.Users.Returns(_users);
        _sut = new VerifyEmailCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserNotFoundByToken_ReturnsInvalidOrExpiredToken()
    {
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns((User?)null);

        var result = await _sut.Handle(new VerifyEmailCommand("token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidOrExpiredToken);
    }

    [Fact]
    public async Task Handle_WhenTokenExpired_ReturnsInvalidOrExpiredToken()
    {
        var user = AuthUserTestData.Entity(isEmailVerified: false);
        user.EmailVerificationTokenExpiryTime = DateTime.UtcNow.AddMinutes(-1);

        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns(user);

        var result = await _sut.Handle(new VerifyEmailCommand("token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.InvalidOrExpiredToken);
    }

    [Fact]
    public async Task Handle_WhenValid_VerifiesEmailAndClearsToken()
    {
        var user = AuthUserTestData.Entity(isEmailVerified: false);
        user.EmailVerificationToken = "token";
        user.EmailVerificationTokenExpiryTime = DateTime.Now.AddMinutes(30);

        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns(user);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new VerifyEmailCommand("token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
        user.EmailVerificationToken.Should().BeNull();
        user.EmailVerificationTokenExpiryTime.Should().BeNull();
        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

