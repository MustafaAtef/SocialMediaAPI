using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Dtos;
using SocialMedia.Application.Posts.Commands.Restore;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Posts;
using SocialMedia.Core.Exceptions;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests.Posts;

public class RestorePostCommandHandlerTests
{
    private readonly RestorePostCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _posts;

    public RestorePostCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _posts = Substitute.For<IPostRepository>();

        _unitOfWork.Posts.Returns(_posts);
        _sut = new RestorePostCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((UserDto?)null);

        var result = await _sut.Handle(new RestorePostCommand(10), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenDeletedPostNotFound_ReturnsPostNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(PostTestData.AuthenticatedUser());
        _posts.GetDeletedPostAsync(999).Returns((Post?)null);

        var result = await _sut.Handle(new RestorePostCommand(999), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUserNotOwner_ThrowsUnauthorized()
    {
        _userService.GetAuthenticatedUser().Returns(PostTestData.AuthenticatedUser(id: 5));
        _posts.GetDeletedPostAsync(1).Returns(new Post { Id = 1, UserId = 3, IsDeleted = true });

        var act = () => _sut.Handle(new RestorePostCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<UnAuthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenValid_RestoresPostAndReturnsResponse()
    {
        var user = PostTestData.AuthenticatedUser(id: 3);
        _userService.GetAuthenticatedUser().Returns(user);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var post = new Post
        {
            Id = 22,
            UserId = 3,
            Content = "deleted",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            Attachments = null,
            ReactionsCount = 6,
            CommentsCount = 2
        };

        _posts.GetDeletedPostAsync(22).Returns(post);

        var result = await _sut.Handle(new RestorePostCommand(22), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        post.IsDeleted.Should().BeFalse();
        post.DeletedAt.Should().BeNull();

        result.Value.Id.Should().Be(22);
        result.Value.Content.Should().Be("deleted");
        result.Value.Attachments.Should().BeEmpty();
        result.Value.Author.Id.Should().Be(user.Id);
        result.Value.ReactsCount.Should().Be(6);
        result.Value.CommentsCount.Should().Be(2);
        result.Value.DeletedAt.Should().BeNull();

        var domainEvent = post.GetDomainEvents().OfType<PostRestoredDomainEvent>().Single();
        domainEvent.PostId.Should().Be(22);

        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

