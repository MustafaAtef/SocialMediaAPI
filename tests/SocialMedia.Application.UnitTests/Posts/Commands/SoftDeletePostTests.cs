using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Dtos;
using SocialMedia.Application.Posts.Commands.SoftDelete;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Posts;
using SocialMedia.Core.Exceptions;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests.Posts;

public class SoftDeletePostCommandHandlerTests
{
    private readonly SoftDeletePostCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _posts;

    public SoftDeletePostCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _posts = Substitute.For<IPostRepository>();

        _unitOfWork.Posts.Returns(_posts);
        _sut = new SoftDeletePostCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((UserDto?)null);

        var result = await _sut.Handle(new SoftDeletePostCommand(1), CancellationToken.None);

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

        var result = await _sut.Handle(new SoftDeletePostCommand(77), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
        _ = _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenUserNotOwner_ThrowsUnauthorized()
    {
        _userService.GetAuthenticatedUser().Returns(PostTestData.AuthenticatedUser(id: 2));
        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>())
            .Returns(new Post { Id = 5, UserId = 1 });

        var act = () => _sut.Handle(new SoftDeletePostCommand(5), CancellationToken.None);

        await act.Should().ThrowAsync<UnAuthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenValid_SoftDeletesPostAndRaisesEvent()
    {
        _userService.GetAuthenticatedUser().Returns(PostTestData.AuthenticatedUser(id: 1));
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var post = new Post { Id = 9, UserId = 1, IsDeleted = false, DeletedAt = null };
        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(post);

        var result = await _sut.Handle(new SoftDeletePostCommand(9), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        post.IsDeleted.Should().BeTrue();
        post.DeletedAt.Should().NotBeNull();

        var domainEvent = post.GetDomainEvents().OfType<PostSoftDeletedDomainEvent>().Single();
        domainEvent.PostId.Should().Be(9);

        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

