using MvApplication.Ports;
using MvDomain.Entities;

namespace MvInfrastructure.Payment;

public class PaymentServiceFactory(
  StripePaymentService stripePaymentService,
  PayPalPaymentService payPalPaymentService
) : IPaymentServiceFactory {
  public IPaymentService Create(PaymentProvider provider) {
    return provider switch {
      PaymentProvider.Stripe => stripePaymentService,
      PaymentProvider.PayPal => payPalPaymentService,
      _ => throw new NotSupportedException($"Khong ho tro cong thanh toan {provider}.")
    };
  }
}
