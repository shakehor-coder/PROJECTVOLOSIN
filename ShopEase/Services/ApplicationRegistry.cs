namespace Ecommerce.Services;

public sealed record RegistrySnapshot(string DeliveryNote, long CompletedReports,
    DateTimeOffset? LastReportAt, int LastProductCount);

/// <summary>Shared application registry. Lifetime is one server process.</summary>
public sealed class ApplicationRegistry
{
    private readonly object _gate = new();
    private RegistrySnapshot _state = new("Доставка уточняется при подтверждении заказа", 0, null, 0);
    public RegistrySnapshot Snapshot() { lock (_gate) return _state; }
    public void SetDeliveryNote(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        text = text.Trim();
        if (text.Length is < 1 or > 200) throw new ArgumentException("Use 1–200 characters.", nameof(text));
        lock (_gate) _state = _state with { DeliveryNote = text };
    }
    public void RecordReport(int productCount)
    {
        if (productCount < 0) throw new ArgumentOutOfRangeException(nameof(productCount));
        lock (_gate) _state = _state with
        {
            CompletedReports = checked(_state.CompletedReports + 1),
            LastReportAt = DateTimeOffset.UtcNow, LastProductCount = productCount
        };
    }
}
