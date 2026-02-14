using MediatR;

namespace MvApplication.UseCases.User.GetAllUsers;

public record GetAllUsersQuery : IRequest<IEnumerable<Models.User>>;
