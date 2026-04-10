using FluentValidation;
using MediatR;
using MvApplication.Models;

namespace MvApplication.UseCases.Auth.Refresh;

public record RefreshCommand(string RefreshToken) : IRequest<AuthTokens>;

public class RefreshValidator : AbstractValidator<RefreshCommand>
{
    public RefreshValidator()
    {
        RuleFor(x => x.RefreshToken)
          .NotEmpty().WithMessage("Refresh token là bắt buộc");
    }
}
