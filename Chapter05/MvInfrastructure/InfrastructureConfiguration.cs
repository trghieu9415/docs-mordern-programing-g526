﻿using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MvApplication;
using MvApplication.Behaviors;
using MvApplication.Options;
using MvApplication.Ports;
using MvInfrastructure.Adapters;
using MvInfrastructure.Data;

namespace MvInfrastructure;

public static class InfrastructureConfiguration {
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config) {
      
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(  
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null)
            )
        );

        services.AddOptions<ProductOptions>()
            .Bind(config.GetSection(ProductOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(resolver =>
            resolver.GetRequiredService<IOptions<ProductOptions>>().Value);

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));

        services.AddApplication();

        return services;
    }

    private static IServiceCollection AddApplication(this IServiceCollection services) {
        var applicationAssembly = typeof(IApplicationMarker).Assembly;

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddAutoMapper(_ => { }, applicationAssembly);

        return services;
    }
}