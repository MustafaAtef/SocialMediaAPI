using System.Linq.Expressions;

using FluentAssertions;

using Microsoft.Extensions.Options;

using NSubstitute;

using SocialMedia.Application.Auth.Commands.SendEmailVerification;
using SocialMedia.Application.Options;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class SendEmailVerificationCommandHandlerTests
{
    private readonly SendEmailVerificationCommandHandler _sut;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;
    private readonly IEmailOutboxWriter _emailOutboxWriter;

    public SendEmailVerificationCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _users = Substitute.For<IUserRepository>();
        _emailOutboxWriter = Substitute.For<IEmailOutboxWriter>();

        _unitOfWork.Users.Returns(_users);

        var options = Microsoft.Extensions.Options.Options.Create(new ExpireDurationsOptions
        {
            EmailVerificationTokenExpiryMinutes = 45,
            PasswordResetTokenExpiryMinutes = 30
        });

        _sut = new SendEmailVerificationCommandHandler(_unitOfWork, _emailOutboxWriter, options);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsSuccessWithoutQueueing()
    {
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns((User?)null);

        var result = await _sut.Handle(new SendEmailVerificationCommand("missing@example.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _emailOutboxWriter.DidNotReceive().QueueVerificationEmail(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>());
        _ = _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenAlreadyVerified_ReturnsEmailAlreadyVerifiedError()
    {
        var user = AuthUserTestData.Entity(email: "user@example.com", isEmailVerified: true);
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns(user);

        var result = await _sut.Handle(new SendEmailVerificationCommand("user@example.com"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.EmailAlreadyVerified);
        _emailOutboxWriter.DidNotReceive().QueueVerificationEmail(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task Handle_WhenValid_GeneratesTokenAndQueuesEmail()
    {
        var user = AuthUserTestData.Entity(email: "user@example.com", isEmailVerified: false);
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns(user);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new SendEmailVerificationCommand("user@example.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.EmailVerificationToken.Should().NotBeNullOrWhiteSpace();
        user.EmailVerificationTokenExpiryTime.Should().NotBeNull();

        _emailOutboxWriter.Received(1).QueueVerificationEmail(
            "user@example.com",
            Arg.Any<string>(),
            Arg.Any<DateTime>());
        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

