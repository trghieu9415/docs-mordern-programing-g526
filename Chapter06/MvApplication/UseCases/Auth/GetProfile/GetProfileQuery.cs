using MediatR;

namespace MvApplication.UseCases.Auth.GetProfile;

public record GetProfileQuery : IRequest<Models.User>;
