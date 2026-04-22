using System.Linq.Expressions;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using NSubstitute;

using SocialMedia.Application.Auth.Commands.Register;
using SocialMedia.Application.Options;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Users;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class RegisterCommandHandlerTests
{
    private readonly RegisterCommandHandler _sut;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IFileUploader _fileUploader;
    private readonly IEmailOutboxWriter _emailOutboxWriter;

    public RegisterCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _users = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtService = Substitute.For<IJwtService>();
        _fileUploader = Substitute.For<IFileUploader>();
        _emailOutboxWriter = Substitute.For<IEmailOutboxWriter>();

        _unitOfWork.Users.Returns(_users);

        var options = Microsoft.Extensions.Options.Options.Create(new ExpireDurationsOptions
        {
            EmailVerificationTokenExpiryMinutes = 60,
            PasswordResetTokenExpiryMinutes = 30
        });

        _sut = new RegisterCommandHandler(
            _unitOfWork,
            _passwordHasher,
            _jwtService,
            _fileUploader,
            _emailOutboxWriter,
            options);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ReturnsAlreadyExistsError()
    {
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns(AuthUserTestData.Entity());

        var command = new RegisterCommand("Mostafa", "Atef", "mostafa@example.com", "StrongPass123", null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.AlreadyExists);
        _users.DidNotReceive().Add(Arg.Any<User>());
        _ = _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenValidWithoutAvatar_RegistersUserAndReturnsResponse()
    {
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns((User?)null);

        _passwordHasher.HashPassword("StrongPass123").Returns("HASHED");
        var jwt = AuthUserTestData.Jwt(token: "jwt-token", refreshToken: "jwt-refresh");
        _jwtService.GenerateToken(Arg.Any<User>()).Returns(jwt);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        User? addedUser = null;
        _users.When(x => x.Add(Arg.Any<User>())).Do(ci => addedUser = ci.Arg<User>());

        var command = new RegisterCommand("Mostafa", "Atef", "mostafa@example.com", "StrongPass123", null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("mostafa@example.com");
        result.Value.Name.Should().Be("Mostafa Atef");
        result.Value.Token.Should().Be("jwt-token");
        result.Value.RefreshToken.Should().Be("jwt-refresh");
        result.Value.AvatarUrl.Should().BeEmpty();

        addedUser.Should().NotBeNull();
        addedUser!.Password.Should().Be("HASHED");
        addedUser.EmailVerificationToken.Should().NotBeNullOrWhiteSpace();
        addedUser.EmailVerificationTokenExpiryTime.Should().NotBeNull();

        var domainEvent = addedUser.GetDomainEvents().OfType<UserRegisteredDomainEvent>().Single();
        domainEvent.UserEmail.Should().Be("mostafa@example.com");
        domainEvent.UserAvatarUrl.Should().BeEmpty();

        _emailOutboxWriter.Received(1).QueueVerificationEmail(
            "mostafa@example.com",
            Arg.Any<string>(),
            Arg.Any<DateTime>());
        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenAvatarProvided_UploadsAvatarAndReturnsAvatarUrl()
    {
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>())
            .Returns((User?)null);

        _passwordHasher.HashPassword("StrongPass123").Returns("HASHED");
        var jwt = AuthUserTestData.Jwt();
        _jwtService.GenerateToken(Arg.Any<User>()).Returns(jwt);

        var avatar = Substitute.For<IFormFile>();
        _fileUploader.UploadAsync(avatar, "users-avatars")
            .Returns(new SocialMedia.Application.Dtos.UploadedFileDto
            {
                StorageProvider = StorageProvider.Supabase,
                Url = "https://cdn/avatar.jpg"
            });

        User? addedUser = null;
        _users.When(x => x.Add(Arg.Any<User>())).Do(ci => addedUser = ci.Arg<User>());

        var command = new RegisterCommand("Mostafa", "Atef", "mostafa@example.com", "StrongPass123", avatar);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AvatarUrl.Should().Be("https://cdn/avatar.jpg");

        _ = _fileUploader.Received(1).UploadAsync(avatar, "users-avatars");
        addedUser.Should().NotBeNull();
        addedUser!.Avatar.Should().NotBeNull();
        addedUser.Avatar!.Url.Should().Be("https://cdn/avatar.jpg");
        addedUser.Avatar.StorageProvider.Should().Be(StorageProvider.Supabase);
    }
}

