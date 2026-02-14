using MediatR;

namespace MvApplication.UseCases.Auth.Logout;

public record LogoutCommand(string? RefreshToken) : IRequest;
