namespace MvApplication.Options;

/// <summary>
/// Cau hinh demo tranh chap / 429. Dat <see cref="PurchaseHandlerDelayMs"/> lon hon WaitTime cua lenh mua (1 giay)
/// de cac request song song nhan 429.
/// </summary>
public class FlashSaleDemoOptions {
  public const string SectionName = "FlashSaleDemo";

  /// <summary>0 = tat. Vi du 1500 de giu lock > 1s khi test parallel.</summary>
  public int PurchaseHandlerDelayMs { get; set; }
}
