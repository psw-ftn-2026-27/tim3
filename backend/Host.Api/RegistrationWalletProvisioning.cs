namespace Host.Api;

public sealed class RegistrationWalletProvisioning
{
    private readonly Func<Guid, Task> _ensureWalletAsync;

    public RegistrationWalletProvisioning(Func<Guid, Task> ensureWalletAsync)
    {
        _ensureWalletAsync = ensureWalletAsync;
    }

    public Task EnsureAsync(Guid userId)
    {
        return _ensureWalletAsync(userId);
    }
}
