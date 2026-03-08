using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Users;

namespace SocialMedia.Application.Users.Commands.Update;

public class UpdateUserCommandHandler(IUserService userService, IUnitOfWork unitOfWork, IFileUploader fileUploader)
    : ICommandHandler<UpdateUserCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var tokenUser = userService.GetAuthenticatedUser();
        if (tokenUser is null)
            return Result.Failure<UserDto>(UserErrors.Unauthenticated);

        var user = await unitOfWork.Users.GetAsync(u => u.Id == tokenUser.Id, ["Avatar"]);
        if (user is null)
            return Result.Failure<UserDto>(UserErrors.NotFound);

        var oldAvatar = user.Avatar;

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;

        if (request.Avatar is not null)
        {
            var uploadedAvatar = await fileUploader.UploadAsync(request.Avatar, "users-avatars");
            user.Avatar = new Avatar
            {
                StorageProvider = uploadedAvatar.StorageProvider,
                Url = uploadedAvatar.Url
            };
        }

        user.RaiseDomainEvent(() => new UserUpdatedDomainEvent(
            user.Id,
            $"{user.FirstName} {user.LastName}",
            user.Avatar?.Url ?? string.Empty));
        await unitOfWork.SaveChangesAsync();

        if (request.Avatar is not null && oldAvatar is not null)
            await fileUploader.DeleteAsync(oldAvatar.Url);

        return Result.Success(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = $"{user.FirstName} {user.LastName}",
            AvatarUrl = user.Avatar?.Url ?? string.Empty
        });
    }
}
