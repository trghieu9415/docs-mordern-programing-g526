using FluentValidation;

namespace MvApplication.UseCases.UpdateProduct;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand> {
  public UpdateProductValidator() {
    RuleFor(x => x.Id)
      .NotEmpty().WithMessage("ID sản phẩm là bắt buộc.");

    RuleFor(x => x.Name)
      .NotEmpty().WithMessage("Tên sản phẩm không được để trống.")
      .MinimumLength(3).WithMessage("Tên sản phẩm phải có ít nhất 3 ký tự.")
      .MaximumLength(200).WithMessage("Tên sản phẩm không được vượt quá 200 ký tự.");

    RuleFor(x => x.Price)
      .GreaterThanOrEqualTo(0).WithMessage("Giá sản phẩm không được nhỏ hơn 0.");

    RuleFor(x => x.ImageUrl)
      .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
      .WithMessage("Đường dẫn hình ảnh không hợp lệ (phải là URL tuyệt đối).");
  }
}
