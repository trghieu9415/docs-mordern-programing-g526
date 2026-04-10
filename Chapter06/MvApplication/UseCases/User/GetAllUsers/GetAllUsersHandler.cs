using MediatR;
using MvApplication.Ports.Security;

namespace MvApplication.UseCases.User.GetAllUsers;

public class GetAllUsersHandler(IUserService userService) : IRequestHandler<GetAllUsersQuery, IEnumerable<Models.User>>
{

    public async Task<IEnumerable<Models.User>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        return await userService.GetAllAsync(ct);
    }
}
