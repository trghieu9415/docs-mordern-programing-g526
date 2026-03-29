using MvDomain.Entities;

namespace MvApplication.Ports;

public interface IPaymentServiceFactory {
  IPaymentService Create(PaymentProvider provider);
}
