using Shared.Domain;

namespace Payment.Application.Wallets;

public sealed record WalletDto(int Balance, PageResult<WalletTransactionDto> Transactions);
