using Shared.Domain;

namespace Payment.Application.OwnWallet;

public sealed record WalletDto(int Balance, PageResult<WalletTransactionDto> Transactions);
