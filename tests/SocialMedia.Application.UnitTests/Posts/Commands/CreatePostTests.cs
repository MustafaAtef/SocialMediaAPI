using FluentAssertions;

using Microsoft.AspNetCore.Http;

using NSubstitute;

using SocialMedia.Application.Dtos;
using SocialMedia.Application.Posts.Commands.Create;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Posts;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests.Posts;

public class CreatePostCommandHandlerTests
{
    private readonly CreatePostCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IFileUploader _fileUploader;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _posts;

    public CreatePostCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _fileUploader = Substitute.For<IFileUploader>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _posts = Substitute.For<IPostRepository>();

        _unitOfWork.Posts.Returns(_posts);
        _sut = new CreatePostCommandHandler(_userService, _fileUploader, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((UserDto?)null);
        var command = new CreatePostCommand("Hello", new List<IFormFile>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
        _posts.DidNotReceive().Add(Arg.Any<Post>());
        _ = _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenValidAndNoAttachments_CreatesPostAndReturnsResponse()
    {
        var user = PostTestData.AuthenticatedUser();
        _userService.GetAuthenticatedUser().Returns(user);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        Post? addedPost = null;
        _posts.When(x => x.Add(Arg.Any<Post>())).Do(ci => addedPost = ci.Arg<Post>());

        var command = new CreatePostCommand("First post", new List<IFormFile>());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("First post");
        result.Value.Author.Id.Should().Be(user.Id);
        result.Value.Author.Name.Should().Be(user.Name);
        result.Value.ReactsCount.Should().Be(0);
        result.Value.CommentsCount.Should().Be(0);
        result.Value.Attachments.Should().BeEmpty();

        addedPost.Should().NotBeNull();
        addedPost!.UserId.Should().Be(user.Id);
        addedPost.Content.Should().Be("First post");

        var createdEvent = addedPost.GetDomainEvents().OfType<PostCreatedDomainEvent>().Single();
        createdEvent.UserId.Should().Be(user.Id);
        createdEvent.Content.Should().Be("First post");
        createdEvent.Attachments.Should().BeEmpty();

        _ = _fileUploader.DidNotReceive().UploadAsync(Arg.Any<IFormFile>(), Arg.Any<string>());
        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenAttachmentsProvided_UploadsAndMapsThem()
    {
        var user = PostTestData.AuthenticatedUser();
        _userService.GetAuthenticatedUser().Returns(user);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var file1 = Substitute.For<IFormFile>();
        var file2 = Substitute.For<IFormFile>();

        _fileUploader.UploadAsync(file1, "posts-attachments")
            .Returns(Task.FromResult(new UploadedFileDto
            {
                Type = AttachmentType.Image,
                StorageProvider = StorageProvider.Server,
                Url = "https://cdn/a.jpg"
            }));

        _fileUploader.UploadAsync(file2, "posts-attachments")
            .Returns(Task.FromResult(new UploadedFileDto
            {
                Type = AttachmentType.Video,
                StorageProvider = StorageProvider.Supabase,
                Url = "https://cdn/b.mp4"
            }));

        Post? addedPost = null;
        _posts.When(x => x.Add(Arg.Any<Post>())).Do(ci => addedPost = ci.Arg<Post>());

        var command = new CreatePostCommand("With files", new List<IFormFile> { file1, file2 });

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Attachments.Should().HaveCount(2);
        result.Value.Attachments.Select(x => x.Url).Should().Contain(new[] { "https://cdn/a.jpg", "https://cdn/b.mp4" });

        _ = _fileUploader.Received(1).UploadAsync(file1, "posts-attachments");
        _ = _fileUploader.Received(1).UploadAsync(file2, "posts-attachments");

        var createdEvent = addedPost!.GetDomainEvents().OfType<PostCreatedDomainEvent>().Single();
        createdEvent.Attachments.Should().HaveCount(2);
        createdEvent.Attachments.Select(x => x.AttachmentType)
            .Should().Contain(new[] { AttachmentType.Image, AttachmentType.Video });
    }
}

