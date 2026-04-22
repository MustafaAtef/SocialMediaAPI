using System.Linq.Expressions;

using FluentAssertions;

using NSubstitute;

using SocialMedia.Application.Comments.Commands.Update;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Comments;
using SocialMedia.Core.RepositoryContracts;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class UpdateCommentCommandHandlerTests
{
    private readonly UpdateCommentCommandHandler _sut;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICommentRepository _comments;

    public UpdateCommentCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _comments = Substitute.For<ICommentRepository>();

        _unitOfWork.Comments.Returns(_comments);
        _sut = new UpdateCommentCommandHandler(_userService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ReturnsUnauthenticatedError()
    {
        _userService.GetAuthenticatedUser().Returns((SocialMedia.Application.Dtos.UserDto?)null);

        var result = await _sut.Handle(new UpdateCommentCommand(1, 2, "updated"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.Unauthenticated);
    }

    [Fact]
    public async Task Handle_WhenCommentNotFound_ReturnsCommentNotFound()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser());
        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>()).Returns((Comment?)null);

        var result = await _sut.Handle(new UpdateCommentCommand(99, 1, "updated"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUnauthorized_ReturnsUnauthorizedError()
    {
        _userService.GetAuthenticatedUser().Returns(CommentUserTestData.AuthenticatedUser(id: 8));
        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>())
            .Returns(new Comment { Id = 5, PostId = 1, UserId = 3, Content = "old" });

        var result = await _sut.Handle(new UpdateCommentCommand(5, 1, "updated"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesAndReturnsResponse()
    {
        var user = CommentUserTestData.AuthenticatedUser(id: 3);
        _userService.GetAuthenticatedUser().Returns(user);

        var comment = new Comment
        {
            Id = 5,
            PostId = 1,
            UserId = 3,
            Content = "old",
            ReactionsCount = 4,
            RepliesCount = 2,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        _comments.GetAsync(Arg.Any<Expression<Func<Comment, bool>>>(), Arg.Any<string[]?>()).Returns(comment);
        _unitOfWork.SaveChangesAsync().Returns(Task.FromResult(1));

        var result = await _sut.Handle(new UpdateCommentCommand(5, 1, "updated"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(5);
        result.Value.Content.Should().Be("updated");
        result.Value.ReactsCount.Should().Be(4);
        result.Value.RepliesCount.Should().Be(2);
        result.Value.CreatedBy.Id.Should().Be(3);

        comment.Content.Should().Be("updated");
        comment.UpdatedAt.Should().NotBeNull();

        var domainEvent = comment.GetDomainEvents().OfType<CommentUpdatedDomainEvent>().Single();
        domainEvent.CommentId.Should().Be(5);
        domainEvent.Content.Should().Be("updated");

        _ = _unitOfWork.Received(1).SaveChangesAsync();
    }
}

