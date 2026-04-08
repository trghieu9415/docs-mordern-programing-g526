using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MvApplication;
using MvApplication.Behaviors;

namespace MvInfrastructure.Extensions;

public static class MediatorExtensions {
  public static IServiceCollection AddMediatorPipeline(this IServiceCollection services) {
    var applicationAssembly = typeof(IApplicationMarker).Assembly;

    services.AddMediatR(cfg => {
      cfg.RegisterServicesFromAssembly(applicationAssembly);

      cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
      cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
    });

    services.AddValidatorsFromAssembly(applicationAssembly);
    return services;
  }
}
