using Payment.Domain.Wallets;

namespace Payment.Application.Wallets;

public sealed record WalletTransactionDto(Guid Id, int Amount, DateTime OccurredAt, WalletTransactionKind Kind);
