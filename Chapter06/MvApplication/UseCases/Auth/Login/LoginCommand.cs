using FluentValidation;
using MediatR;
using MvApplication.Models;

namespace MvApplication.UseCases.Auth.Login;

public record LoginCommand(
  string Email,
  string Password
) : IRequest<AuthTokens>;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
          .NotEmpty().WithMessage("Email là bắt buộc")
          .EmailAddress().WithMessage("Email không hợp lệ");

        RuleFor(x => x.Password)
          .NotEmpty().WithMessage("Password là bắt buộc");
    }
}
