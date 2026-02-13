using System.ComponentModel.DataAnnotations;

namespace MvApplication.Options;

public class ProductOptions {
  public const string SectionName = "ProductSettings";

  [Range(1, 1000, ErrorMessage = "Số lượng tối đa trên trang phải từ 1 đến 1000.")]
  public int MaxItemsPerPage { get; set; } = 50;

  [Range(1, 100, ErrorMessage = "Số lượng mặc định phải từ 1 đến 100.")]
  public int DefaultItemsPerPage { get; set; } = 20;

  [Required(ErrorMessage = "Cấu trúc SKU không được để trống.")]
  public string SkuPattern { get; set; } = string.Empty;

  public PriceLimitOptions PriceLimit { get; set; } = new();

  [Range(0, int.MaxValue)] public int InventoryThreshold { get; set; } = 0;

  public class PriceLimitOptions {
    [Range(0, double.MaxValue, ErrorMessage = "Giá tối thiểu không được âm.")]
    public decimal Min { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá tối đa không được âm.")]
    public decimal Max { get; set; }
  }
}
