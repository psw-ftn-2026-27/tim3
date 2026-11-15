using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.AdminWallet;
using Payment.Application.GiftCardPurchasing;
using Payment.Application.GiftCards;
using Payment.Application.OwnWallet;
using Payment.Application.Wallets;
using Payment.Contracts;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.GiftCards;
using Payment.Infrastructure.Persistence.Wallets;
using Shared.Infrastructure;

namespace Payment.Infrastructure;

public static class PaymentModuleExtensions
{
    public static IServiceCollection AddPaymentModule<TRegistrationWallet>(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<Func<Guid, Task>, TRegistrationWallet> bindRegistrationWallet)
        where TRegistrationWallet : class
    {
        services.AddModuleDbContext<PaymentDbContext>(configuration, "payment");
        services.AddHostedService<PaymentModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<PaymentMapperProfile>());

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentDbContext>());
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletReadRepository, WalletReadRepository>();
        services.AddScoped<OwnWalletService>();
        services.AddScoped<IPaymentApi>(provider => provider.GetRequiredService<OwnWalletService>());
        services.AddScoped(provider =>
            bindRegistrationWallet(provider.GetRequiredService<IPaymentApi>().EnsureWalletAsync));
        services.AddScoped<OwnWalletQueries>();
        services.AddScoped<AdminWalletService>();
        services.AddScoped<AdminWalletQueries>();
        services.AddScoped<IGiftCardRepository, GiftCardRepository>();
        services.AddScoped<IGiftCardReadRepository, GiftCardReadRepository>();
        services.AddScoped<GiftCardPurchasingService>();
        services.AddScoped<GiftCardPurchasingQueries>();

        return services;
    }
}
