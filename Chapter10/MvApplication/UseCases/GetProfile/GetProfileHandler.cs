using MediatR;
using MvApplication.Repositories;
using MvDomain.Entities;

namespace MvApplication.UseCases.GetProfile;

public class GetProfileHandler(
  IRepository<User> userRepository
) : IRequestHandler<GetProfileQuery, GetProfileResult> {
  public async Task<GetProfileResult> Handle(GetProfileQuery request, CancellationToken cancellationToken) {
    return new GetProfileResult(await userRepository.GetByIdAsync(request.UserId, cancellationToken));
  }
}
