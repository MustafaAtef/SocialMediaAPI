using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.UserFollows;

namespace SocialMedia.Application.Users.Commands.Follow;

public class FollowUserCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<FollowUserCommand>
{
    public async Task<Result> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        var tokenUser = userService.GetAuthenticatedUser();
        if (tokenUser is null)
            return Result.Failure(UserErrors.Unauthenticated);

        if (request.UserId == tokenUser.Id)
            return Result.Failure(UserErrors.SelfFollow);

        var followingUser = await unitOfWork.Users.GetAsync(u => u.Id == request.UserId, ["Avatar"]);
        var followerUser = await unitOfWork.Users.GetAsync(u => u.Id == tokenUser.Id, ["Avatar"]);
        if (followingUser is null || followerUser is null)
            return Result.Failure(UserErrors.NotFound);

        var existingFollow = await unitOfWork.FollowersFollowings.GetAsync(
            ff => ff.FollowerId == followerUser.Id && ff.FollowingId == followingUser.Id);
        if (existingFollow != null)
            return Result.Failure(UserErrors.AlreadyFollowing);

        var follow = new FollowerFollowing
        {
            FollowerId = followerUser.Id,
            FollowingId = followingUser.Id
        };
        unitOfWork.FollowersFollowings.Add(follow);
        followerUser.FollowingCount++;
        followingUser.FollowersCount++;
        follow.RaiseDomainEvent(() => new UserFollowedDomainEvent(
            followerUser.Id, followingUser.Id,
            $"{followerUser.FirstName} {followerUser.LastName}", followerUser.Email, followerUser.Avatar?.Url ?? string.Empty,
            $"{followingUser.FirstName} {followingUser.LastName}", followingUser.Email, followingUser.Avatar?.Url ?? string.Empty,
            DateTime.UtcNow));
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
