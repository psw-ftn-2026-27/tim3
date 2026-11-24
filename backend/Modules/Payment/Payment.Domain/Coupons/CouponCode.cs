using System.Security.Cryptography;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

public sealed record CouponCode
{
    private const int Length = 8;
    private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Value { get; private init; } = null!;

    private CouponCode() { }

    public CouponCode(string value)
    {
        if (value is null || value.Length != Length || !value.All(AllowedCharacters.Contains))
        {
            throw new DomainException($"A coupon code has exactly {Length} uppercase letters and digits.");
        }

        Value = value;
    }

    // A buyer may type the code in any case. Text that cannot be a code names no coupon.
    public static CouponCode FromInput(string text)
    {
        var value = text?.ToUpperInvariant();
        if (value is null || value.Length != Length || !value.All(AllowedCharacters.Contains))
        {
            throw new NotFoundException("The coupon does not exist.");
        }

        return new CouponCode(value);
    }

    public static CouponCode Generate()
    {
        return new CouponCode(RandomNumberGenerator.GetString(AllowedCharacters, Length));
    }
}
