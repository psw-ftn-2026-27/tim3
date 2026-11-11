using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.OwnWallet;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallet")]
[Authorize]
public sealed class OwnWalletController : ControllerBase
{
    private readonly OwnWalletQueries _walletQueries;

    public OwnWalletController(OwnWalletQueries walletQueries)
    {
        _walletQueries = walletQueries;
    }

    [HttpGet("me")]
    public async Task<ActionResult<WalletDto>> GetMyWallet(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _walletQueries.GetMyWalletAsync(User.GetUserId(), page, pageSize);
    }
}
