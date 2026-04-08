using FluentValidation;
using MediatR;
using MvApplication.Abstractions;

namespace MvApplication.UseCases.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResult>, ITransactional;

public record LoginResult(string Token);

public class LoginValidator : AbstractValidator<LoginCommand> {
  public LoginValidator() {
    RuleFor(x => x.Username)
      .NotEmpty().WithMessage("Username không được để trống.")
      .MinimumLength(3).WithMessage("Username phải có ít nhất 3 ký tự.");

    RuleFor(x => x.Password)
      .NotEmpty().WithMessage("Password không được để trống.")
      .MinimumLength(6).WithMessage("Password phải có ít nhất 6 ký tự.");
  }
}
