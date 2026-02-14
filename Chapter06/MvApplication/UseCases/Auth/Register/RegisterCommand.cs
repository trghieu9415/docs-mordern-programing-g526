using FluentValidation;
using MediatR;
using MvApplication.Models;

namespace MvApplication.UseCases.Auth.Register;

public record RegisterCommand(
  string UserName,
  string Email,
  string Password,
  UserRole Role = UserRole.User
) : IRequest<AuthTokens>;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.UserName)
          .NotEmpty().WithMessage("Username là bắt buộc")
          .MinimumLength(3).WithMessage("Username phải có ít nhất 3 ký tự")
          .MaximumLength(50).WithMessage("Username không được quá 50 ký tự");

        RuleFor(x => x.Email)
          .NotEmpty().WithMessage("Email là bắt buộc")
          .EmailAddress().WithMessage("Email không hợp lệ");

        RuleFor(x => x.Password)
          .NotEmpty().WithMessage("Password là bắt buộc")
          .MinimumLength(8).WithMessage("Password phải có ít nhất 8 ký tự")
          .Matches(@"[A-Z]").WithMessage("Password phải có ít nhất 1 chữ hoa")
          .Matches(@"[a-z]").WithMessage("Password phải có ít nhất 1 chữ thường")
          .Matches(@"[0-9]").WithMessage("Password phải có ít nhất 1 số")
          .Matches(@"[\W_]").WithMessage("Password phải có ít nhất 1 ký tự đặc biệt");
    }
}
