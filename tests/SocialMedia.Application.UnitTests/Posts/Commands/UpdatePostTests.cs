using System.Linq.Expressions;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using SocialMedia.Application.Dtos;
using SocialMedia.Application.Posts.Commands.Update;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Posts;
using SocialMedia.Core.Exceptions;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests.Posts;

public class UpdatePostCommandHandlerTests
{
    private readonly UpdatePostCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IFileUploader _fileUploader;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _posts;
    private readonly IServiceScopeFactory _scopeFactory;

    public UpdatePostCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _fileUploader = Substitute.For<IFileUploader>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _posts = Substitute.For<IPostRepository>();
        _scopeFactory = Substitute.For<IServiceScopeFactory>();

        _unitOfWork.Posts.Returns(_posts);
        _sut = new UpdatePostCommandHandler(_userService, _fileUploader, _unitOfWork, _scopeFactory);
    }

    [Fact]
    public async Task Handle_WhenUserUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((UserDto?)null);
        var command = new UpdatePostCommand(1, "updated", null, null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
        _ = _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ReturnsPostNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(PostTestData.AuthenticatedUser());
        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>())
            .Returns((Post?)null);

        var result = await _sut.Handle(new UpdatePostCommand(999, "updated", null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
        _ = _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenUserNotOwner_ThrowsUnauthorized()
    {
        _userService.GetAuthenticatedUser().Returns(PostTestData.AuthenticatedUser(id: 2));
        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>())
            .Returns(new Post { Id = 10, UserId = 1, Content = "x", Attachments = new List<PostAttachment>() });

        var act = () => _sut.Handle(new UpdatePostCommand(10, "updated", null, null), CancellationToken.None);

        await act.Should().ThrowAsync<UnAuthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenValidContentOnly_UpdatesAndReturnsResponse()
    {
        var user = PostTestData.AuthenticatedUser(id: 5);
        _userService.GetAuthenticatedUser().Returns(user);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var post = new Post
        {
            Id = 7,
            UserId = 5,
            Content = "old",
            ReactionsCount = 3,
            CommentsCount = 4,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            Attachments = new List<PostAttachment>
            {
                new() { Id = 11, Url = "https://cdn/old.jpg", AttachmentType = AttachmentType.Image, StorageProvider = StorageProvider.Server }
            }
        };

        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(post);

        var result = await _sut.Handle(new UpdatePostCommand(7, "new content", null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("new content");
        result.Value.ReactsCount.Should().Be(3);
        result.Value.CommentsCount.Should().Be(4);
        result.Value.Attachments.Should().HaveCount(1);

        post.Content.Should().Be("new content");
        post.UpdatedAt.Should().NotBeNull();

        var domainEvent = post.GetDomainEvents().OfType<PostUpdatedDomainEvent>().Single();
        domainEvent.Content.Should().Be("new content");
        domainEvent.AddedAttachments.Should().BeEmpty();
        domainEvent.RemovedAttachmentIds.Should().BeEmpty();

        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenAddingAttachments_UploadsAndAddsThem()
    {
        var user = PostTestData.AuthenticatedUser(id: 9);
        _userService.GetAuthenticatedUser().Returns(user);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var post = new Post
        {
            Id = 100,
            UserId = 9,
            Content = "base",
            Attachments = new List<PostAttachment>()
        };

        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(post);

        var file1 = Substitute.For<IFormFile>();
        var file2 = Substitute.For<IFormFile>();

        _fileUploader.UploadAsync(file1, "posts-attachments")
            .Returns(Task.FromResult(new UploadedFileDto
            {
                Type = AttachmentType.Image,
                StorageProvider = StorageProvider.Server,
                Url = "https://cdn/1.jpg"
            }));

        _fileUploader.UploadAsync(file2, "posts-attachments")
            .Returns(Task.FromResult(new UploadedFileDto
            {
                Type = AttachmentType.Video,
                StorageProvider = StorageProvider.Supabase,
                Url = "https://cdn/2.mp4"
            }));

        var command = new UpdatePostCommand(100, null, new List<IFormFile> { file1, file2 }, null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        post.Attachments.Should().HaveCount(2);
        result.Value.Attachments.Should().HaveCount(2);

        _ = _fileUploader.Received(1).UploadAsync(file1, "posts-attachments");
        _ = _fileUploader.Received(1).UploadAsync(file2, "posts-attachments");

        var domainEvent = post.GetDomainEvents().OfType<PostUpdatedDomainEvent>().Single();
        domainEvent.AddedAttachments.Should().HaveCount(2);
        domainEvent.RemovedAttachmentIds.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenDeletingExistingAttachments_RemovesAndDeletesFromStorage()
    {
        var user = PostTestData.AuthenticatedUser(id: 3);
        _userService.GetAuthenticatedUser().Returns(user);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var storageUploader = Substitute.For<IFileUploader>();
        var serviceProvider = new SingleKeyedServiceProvider(storageUploader);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);
        _scopeFactory.CreateScope().Returns(scope);

        var post = new Post
        {
            Id = 55,
            UserId = 3,
            Content = "content",
            Attachments = new List<PostAttachment>
            {
                new() { Id = 1, Url = "https://cdn/1.jpg", StorageProvider = StorageProvider.Server, AttachmentType = AttachmentType.Image },
                new() { Id = 2, Url = "https://cdn/2.jpg", StorageProvider = StorageProvider.Server, AttachmentType = AttachmentType.Image }
            }
        };

        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(post);

        var command = new UpdatePostCommand(55, null, null, new List<int> { 1, 999 });

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        post.Attachments!.Select(x => x.Id).Should().BeEquivalentTo(new[] { 2 });
        _ = storageUploader.Received(1).DeleteAsync("https://cdn/1.jpg");
        _scopeFactory.Received(1).CreateScope();

        var domainEvent = post.GetDomainEvents().OfType<PostUpdatedDomainEvent>().Single();
        domainEvent.RemovedAttachmentIds.Should().BeEquivalentTo(new[] { 1 });
        domainEvent.AddedAttachments.Should().BeEmpty();
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

