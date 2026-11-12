using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.OwnWallet;
using Payment.Application.Wallets;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallet")]
[Authorize]
public sealed class OwnWalletController : ControllerBase
{
    private readonly OwnWalletService _walletService;
    private readonly OwnWalletQueries _walletQueries;

    public OwnWalletController(OwnWalletService walletService, OwnWalletQueries walletQueries)
    {
        _walletService = walletService;
        _walletQueries = walletQueries;
    }

    [HttpPost("me")]
    public async Task<ActionResult> EnsureMyWallet()
    {
        await _walletService.EnsureWalletAsync(User.GetUserId());
        return NoContent();
    }

    [HttpGet("me")]
    public async Task<ActionResult<WalletDto>> GetMyWallet(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _walletQueries.GetMyWalletAsync(User.GetUserId(), page, pageSize);
    }
}
