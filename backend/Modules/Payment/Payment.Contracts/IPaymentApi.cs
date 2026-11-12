namespace Payment.Contracts;

public interface IPaymentApi
{
    Task EnsureWalletAsync(Guid userId);
}
