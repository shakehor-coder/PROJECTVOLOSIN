namespace Ecommerce.Services;

/// <summary>One CPU-intensive inventory calculation at a time per application instance.</summary>
public sealed class InventoryCalculationGate
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> calculation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(calculation);
        // Wait outside try: a cancelled waiter never acquired a permit and must not release one.
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await calculation(cancellationToken);
        }
        finally
        {
            // Also release after exceptions and cancellation inside the calculation.
            _semaphore.Release();
        }
    }
}
