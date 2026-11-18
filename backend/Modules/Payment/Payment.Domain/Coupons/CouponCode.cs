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

    public static CouponCode Generate()
    {
        return new CouponCode(RandomNumberGenerator.GetString(AllowedCharacters, Length));
    }
}
