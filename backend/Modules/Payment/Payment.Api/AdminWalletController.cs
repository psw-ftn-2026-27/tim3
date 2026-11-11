using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.AdminWallet;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallet")]
[Authorize(Roles = "administrator")]
public sealed class AdminWalletController : ControllerBase
{
    private readonly AdminWalletService _walletService;

    public AdminWalletController(AdminWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpPost("{userId:guid}/deposits")]
    public async Task<ActionResult> DepositEC(Guid userId, DepositEcDto deposit)
    {
        await _walletService.DepositECAsync(userId, User.GetUserId(), deposit);
        return NoContent();
    }
}
