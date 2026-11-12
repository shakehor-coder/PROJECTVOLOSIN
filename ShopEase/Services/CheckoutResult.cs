namespace Ecommerce.Services;

public sealed record CheckoutResult(int? OrderId, string? Error, bool InvalidAddress = false);
