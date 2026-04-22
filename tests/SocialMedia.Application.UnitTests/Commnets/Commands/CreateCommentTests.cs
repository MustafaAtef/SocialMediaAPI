using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Comments.Commands.Create;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Comments;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class CreateCommentCommandHandlerTests
{
    private readonly CreateCommentCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _posts;
    private readonly ICommentRepository _comments;

    public CreateCommentCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _posts = Substitute.For<IPostRepository>();
        _comments = Substitute.For<ICommentRepository>();

        _unitOfWork.Posts.Returns(_posts);
        _unitOfWork.Comments.Returns(_comments);
        _sut = new CreateCommentCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((SocialMedia.Application.Dtos.UserDto?)null);

        var result = await _sut.Handle(new CreateCommentCommand(1, "Hello"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
        _comments.DidNotReceive().Add(Arg.Any<Comment>());
        _ = _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ReturnsPostNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser());
        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns((Post?)null);

        var result = await _sut.Handle(new CreateCommentCommand(10, "Hello"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.NotFound);
        _comments.DidNotReceive().Add(Arg.Any<Comment>());
        _ = _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenValid_CreatesCommentAndReturnsResponse()
    {
        var user = CommentUserTestData.AuthenticatedUser(id: 7);
        _userService.GetAuthenticatedUser().Returns(user);

        var post = new Post { Id = 2, CommentsCount = 4 };
        _posts.GetAsync(Arg.Any<Expression<Func<Post, bool>>>(), Arg.Any<string[]?>()).Returns(post);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        Comment? addedComment = null;
        _comments.When(x => x.Add(Arg.Any<Comment>())).Do(ci => addedComment = ci.Arg<Comment>());

        var result = await _sut.Handle(new CreateCommentCommand(2, "nice post"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PostId.Should().Be(2);
        result.Value.Content.Should().Be("nice post");
        result.Value.CreatedBy.Id.Should().Be(7);
        result.Value.ReactsCount.Should().Be(0);
        result.Value.RepliesCount.Should().Be(0);

        addedComment.Should().NotBeNull();
        addedComment!.UserId.Should().Be(7);
        addedComment.PostId.Should().Be(2);
        addedComment.Content.Should().Be("nice post");

        post.CommentsCount.Should().Be(5);

        var domainEvent = addedComment.GetDomainEvents().OfType<CommentCreatedDomainEvent>().Single();
        domainEvent.UserId.Should().Be(7);
        domainEvent.PostId.Should().Be(2);
        domainEvent.Content.Should().Be("nice post");

        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

