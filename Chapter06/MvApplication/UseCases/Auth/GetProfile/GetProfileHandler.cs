using MediatR;
using MvApplication.Ports.Security;

namespace MvApplication.UseCases.Auth.GetProfile;

public class GetProfileHandler(ICurrentUser currentUser) : IRequestHandler<GetProfileQuery, Models.User>
{

    public Task<Models.User> Handle(GetProfileQuery request, CancellationToken ct)
    {
        return Task.FromResult(currentUser.User);
    }
}
