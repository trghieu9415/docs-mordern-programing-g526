using System.ComponentModel.DataAnnotations;
using Mv.Infrastructure.Configs;

namespace MvApplication.Configs.Options;

public class CinemaSettings : IOptionSection {
  [Range(1, 10, ErrorMessage = "Tối đa mỗi lượt đặt chỉ từ 1 đến 10 vé!")]
  public int MaxTicketsPerBooking { get; init; }

  [Range(10000, double.MaxValue, ErrorMessage = "Giá vé tối thiểu 10.000đ.")]
  public decimal BasePrice { get; init; }

  [Range(0, double.MaxValue, ErrorMessage = "Phụ thu cuối tuần không được âm.")]
  public decimal WeekendSurcharge { get; init; }

  public static string SectionName => "CinemaSettings";
}
