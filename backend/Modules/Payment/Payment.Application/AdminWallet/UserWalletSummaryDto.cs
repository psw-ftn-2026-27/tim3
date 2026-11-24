namespace Payment.Application.AdminWallet;

public sealed record UserWalletSummaryDto(Guid UserId, string DisplayName, int Balance);
