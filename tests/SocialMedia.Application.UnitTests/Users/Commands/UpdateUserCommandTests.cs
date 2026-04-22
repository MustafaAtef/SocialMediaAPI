using System.Linq.Expressions;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using SocialMedia.Application.ServiceContracts;
using SocialMedia.Application.Users.Commands.Update;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Users;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class UpdateUserCommandHandlerTests
{
    private readonly UpdateUserCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;
    private readonly IFileUploader _fileUploader;
    private readonly IServiceScopeFactory _scopeFactory;

    public UpdateUserCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _users = Substitute.For<IUserRepository>();
        _fileUploader = Substitute.For<IFileUploader>();
        _scopeFactory = Substitute.For<IServiceScopeFactory>();

        _unitOfWork.Users.Returns(_users);
        _sut = new UpdateUserCommandHandler(_userService, _unitOfWork, _fileUploader, _scopeFactory);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((SocialMedia.Application.Dtos.UserDto?)null);

        var result = await _sut.Handle(new UpdateUserCommand("New", "Name", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 5));
        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>()).Returns((User?)null);

        var result = await _sut.Handle(new UpdateUserCommand("New", "Name", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUpdatingWithoutAvatar_UpdatesNamesOnly()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 5));

        var user = AuthUserTestData.Entity(id: 5, firstName: "Old", lastName: "Name", email: "u@example.com");
        user.Avatar = new Avatar { StorageProvider = StorageProvider.Server, Url = "https://cdn/old.jpg" };

        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>()).Returns(user);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new UpdateUserCommand("New", "User", null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New User");
        result.Value.Email.Should().Be("u@example.com");
        result.Value.AvatarUrl.Should().Be("https://cdn/old.jpg");

        user.FirstName.Should().Be("New");
        user.LastName.Should().Be("User");

        _ = _fileUploader.DidNotReceive().UploadAsync(Arg.Any<IFormFile>(), Arg.Any<string?>());
        _scopeFactory.DidNotReceive().CreateScope();

        var domainEvent = user.GetDomainEvents().OfType<UserUpdatedDomainEvent>().Single();
        domainEvent.UserId.Should().Be(5);
        domainEvent.UserName.Should().Be("New User");
    }

    [Fact]
    public async Task Handle_WhenAvatarUpdated_DeletesOldAvatarViaKeyedUploader()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 5));

        var user = AuthUserTestData.Entity(id: 5, firstName: "Old", lastName: "Name", email: "u@example.com");
        user.Avatar = new Avatar { StorageProvider = StorageProvider.Server, Url = "https://cdn/old.jpg" };

        _users.GetAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<string[]?>()).Returns(user);

        var avatarFile = Substitute.For<IFormFile>();
        _fileUploader.UploadAsync(avatarFile, "users-avatars")
            .Returns(new SocialMedia.Application.Dtos.UploadedFileDto
            {
                StorageProvider = StorageProvider.Supabase,
                Url = "https://cdn/new.jpg"
            });

        var oldStorageUploader = Substitute.For<IFileUploader>();
        var serviceProvider = new SingleKeyedServiceProvider(oldStorageUploader);
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);
        _scopeFactory.CreateScope().Returns(scope);

        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new UpdateUserCommand(null, null, avatarFile), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AvatarUrl.Should().Be("https://cdn/new.jpg");

        _ = _fileUploader.Received(1).UploadAsync(avatarFile, "users-avatars");
        _scopeFactory.Received(1).CreateScope();
        _ = oldStorageUploader.Received(1).DeleteAsync("https://cdn/old.jpg");

        user.Avatar.Should().NotBeNull();
        user.Avatar!.StorageProvider.Should().Be(StorageProvider.Supabase);
        user.Avatar.Url.Should().Be("https://cdn/new.jpg");
    }

    private sealed class SingleKeyedServiceProvider(IFileUploader uploader) : IServiceProvider, IKeyedServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            return null;
        }

        public object? GetKeyedService(Type serviceType, object? serviceKey)
        {
            if (serviceType == typeof(IFileUploader) && Equals(serviceKey, StorageProvider.Server.ToString()))
            {
                return uploader;
            }

            return null;
        }

        public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
        {
            return GetKeyedService(serviceType, serviceKey)
                ?? throw new InvalidOperationException("Required keyed service was not found.");
        }
    }
}

