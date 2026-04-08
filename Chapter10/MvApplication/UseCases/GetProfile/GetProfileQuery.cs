using MediatR;
using MvDomain.Entities;

namespace MvApplication.UseCases.GetProfile;

public record GetProfileQuery(
  Guid UserId
) : IRequest<GetProfileResult>;

public record GetProfileResult(User? User);
