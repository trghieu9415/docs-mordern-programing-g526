using FluentValidation;
using MediatR;
using MvApplication.Abstractions;

namespace MvApplication.UseCases.Register;

public record RegisterCommand(string Username, string Email, string Password)
  : IRequest<RegisterResult>, ITransactional;

public record RegisterResult(string Token);

public class RegisterValidator : AbstractValidator<RegisterCommand> {
  public RegisterValidator() {
    RuleFor(x => x.Username)
      .NotEmpty().WithMessage("Username không được để trống.")
      .MinimumLength(3).WithMessage("Username phải có ít nhất 3 ký tự.")
      .MaximumLength(50).WithMessage("Username không được vượt quá 50 ký tự.");

    RuleFor(x => x.Email)
      .NotEmpty().WithMessage("Email không được để trống.")
      .MaximumLength(255)
      .EmailAddress().WithMessage("Email không hợp lệ.");

    RuleFor(x => x.Password)
      .NotEmpty().WithMessage("Password không được để trống.")
      .MinimumLength(6).WithMessage("Password phải có ít nhất 6 ký tự.")
      .MaximumLength(100).WithMessage("Password không được vượt quá 100 ký tự.");
  }
}
