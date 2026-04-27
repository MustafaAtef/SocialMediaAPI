using System.Linq.Expressions;

using FluentAssertions;

using Microsoft.Extensions.Options;

using NSubstitute;

using SocialMedia.Application.Auth.Commands.ForgetPassword;
using SocialMedia.Application.Options;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

[Trait("Category", "Unit")]
public class ForgetPasswordCommandHandlerTests
{
    private readonly ForgetPasswordCommandHandler _sut;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;
    private readonly IEmailOutboxWriter _emailOutboxWriter;

    public ForgetPasswordCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _users = Substitute.For<IUserRepository>();
        _emailOutboxWriter = Substitute.For<IEmailOutboxWriter>();

        _unitOfWork.Users.Returns(_users);

        var options = Microsoft.Extensions.Options.Options.Create(new ExpireDurationsOptions
        {
            EmailVerificationTokenExpiryMinutes = 60,
            PasswordResetTokenExpiryMinutes = 30
        });

        _sut = new ForgetPasswordCommandHandler(_unitOfWork, _emailOutboxWriter, options);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsSuccessWithoutQueueingEmail()
    {
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns((User?)null);

        var result = await _sut.Handle(new ForgetPasswordCommand("missing@example.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _emailOutboxWriter.DidNotReceive().QueuePasswordResetEmail(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>());
        _ = _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenUserExists_GeneratesTokenAndQueuesEmail()
    {
        var user = AuthUserTestData.Entity(email: "user@example.com");
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns(user);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new ForgetPasswordCommand("user@example.com"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.PasswordResetToken.Should().NotBeNullOrWhiteSpace();
        user.PasswordResetTokenExpiryTime.Should().NotBeNull();

        _emailOutboxWriter.Received(1).QueuePasswordResetEmail(
            "user@example.com",
            Arg.Any<string>(),
            Arg.Any<DateTime>());
        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

