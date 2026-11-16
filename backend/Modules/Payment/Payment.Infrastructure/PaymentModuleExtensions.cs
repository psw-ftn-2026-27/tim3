using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.AdminWallet;
using Payment.Application.OwnWallet;
using Payment.Application.Products;
using Payment.Application.ShoppingCart;
using Payment.Application.TourOffering;
using Payment.Application.Wallets;
using Payment.Domain.ShoppingCarts;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.ShoppingCarts;
using Payment.Infrastructure.Persistence.TourPurchases;
using Payment.Infrastructure.Persistence.Wallets;
using Shared.Infrastructure;

namespace Payment.Infrastructure;

public static class PaymentModuleExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<PaymentDbContext>(configuration, "payment");
        services.AddHostedService<PaymentModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<PaymentMapperProfile>());

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentDbContext>());
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletReadRepository, WalletReadRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<TourOfferingService>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IShoppingCartReadRepository, ShoppingCartReadRepository>();
        services.AddScoped<ITourPurchaseTokenRepository, TourPurchaseTokenRepository>();
        services.AddScoped<CartPurchasing>();
        services.AddScoped<ShoppingCartService>();
        services.AddScoped<ShoppingCartQueries>();
        services.AddScoped<OwnWalletQueries>();
        services.AddScoped<AdminWalletService>();
        services.AddScoped<AdminWalletQueries>();

        return services;
    }
}
