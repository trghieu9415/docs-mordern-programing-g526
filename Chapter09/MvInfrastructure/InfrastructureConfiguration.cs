using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MvApplication;
using MvApplication.Behaviors;
using MvApplication.Ports;
using MvInfrastructure.Adapters;
using MvInfrastructure.Configuration;
using MvInfrastructure.Data;
using MvInfrastructure.Http;
using MvInfrastructure.Payment;

namespace MvInfrastructure;

public static class InfrastructureConfiguration {
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config) {
    AddApplication(services);

    services.AddOptions<EmailOptions>().Bind(config.GetSection(EmailOptions.SectionName)).ValidateOnStart();
    services.AddOptions<ObjectStorageOptions>().Bind(config.GetSection(ObjectStorageOptions.SectionName)).ValidateOnStart();
    services.AddOptions<StripeOptions>().Bind(config.GetSection(StripeOptions.SectionName)).ValidateOnStart();
    services.AddOptions<PayPalOptions>().Bind(config.GetSection(PayPalOptions.SectionName)).ValidateOnStart();

    services.AddDbContext<TicketingDbContext>(options =>
      options.UseSqlite(config.GetConnectionString("TicketingDb")));

    services.AddSingleton<IAmazonS3>(serviceProvider => {
      var storageOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ObjectStorageOptions>>().Value;
      var credentials = new BasicAWSCredentials(storageOptions.AccessKey, storageOptions.SecretKey);
      var clientConfig = new AmazonS3Config {
        RegionEndpoint = RegionEndpoint.GetBySystemName(storageOptions.Region),
        ForcePathStyle = storageOptions.ForcePathStyle
      };

      if (!string.IsNullOrWhiteSpace(storageOptions.ServiceUrl)) {
        clientConfig.ServiceURL = storageOptions.ServiceUrl;
      }

      return new AmazonS3Client(credentials, clientConfig);
    });

    services.AddHttpClient("PayPalApi", client => {
        var payPalOptions = config.GetSection(PayPalOptions.SectionName).Get<PayPalOptions>() ?? new PayPalOptions();
        client.BaseAddress = new Uri(payPalOptions.BaseUrl.TrimEnd('/'));
      })
      .AddPolicyHandler((serviceProvider, _) => HttpClientPolicies.CreateRetryPolicy(serviceProvider, "PayPalApi"))
      .AddPolicyHandler((serviceProvider, _) => HttpClientPolicies.CreateCircuitBreakerPolicy(serviceProvider, "PayPalApi"));

    services.AddSingleton(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
    services.AddScoped<IEventRepository, EventRepository>();
    services.AddScoped<ITicketOrderRepository, TicketOrderRepository>();
    services.AddScoped<IStorageService, S3StorageService>();
    services.AddScoped<IEmailService, SmtpEmailService>();
    services.AddScoped<StripePaymentService>();
    services.AddScoped<PayPalPaymentService>();
    services.AddScoped<IPaymentServiceFactory, PaymentServiceFactory>();

    return services;
  }

  private static void AddApplication(IServiceCollection services) {
    var applicationAssembly = typeof(IApplicationMarker).Assembly;

    services.AddMediatR(cfg => {
      cfg.RegisterServicesFromAssembly(applicationAssembly);
      cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    services.AddValidatorsFromAssembly(applicationAssembly);
    services.AddAutoMapper(_ => { }, applicationAssembly);
  }
}
