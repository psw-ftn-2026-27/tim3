using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.AdminWallet;
using Payment.Application.Wallets;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallet")]
[Authorize(Roles = "administrator")]
public sealed class AdminWalletController : ControllerBase
{
    private readonly AdminWalletService _walletService;
    private readonly AdminWalletQueries _walletQueries;

    public AdminWalletController(AdminWalletService walletService, AdminWalletQueries walletQueries)
    {
        _walletService = walletService;
        _walletQueries = walletQueries;
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<WalletDto>> GetUserWallet(
        Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _walletQueries.GetUserWalletAsync(userId, page, pageSize);
    }

    [HttpPost("{userId:guid}/deposits")]
    public async Task<ActionResult> DepositEC(Guid userId, DepositEcDto deposit)
    {
        await _walletService.DepositECAsync(userId, User.GetUserId(), deposit);
        return NoContent();
    }

    [HttpPost("{userId:guid}/deposits/{depositId:guid}/reverse")]
    public async Task<ActionResult> ReverseDepositEC(Guid userId, Guid depositId)
    {
        await _walletService.ReverseDepositECAsync(userId, depositId, User.GetUserId());
        return NoContent();
    }
}
