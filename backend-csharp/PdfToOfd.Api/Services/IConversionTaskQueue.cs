namespace PdfToOfd.Api.Services;

public record ConversionTask(
    long RecordId,
    string PdfPath,
    string OfdPath,
    string FileName
);

public interface IConversionTaskQueue
{
    void QueueBackgroundWorkItem(ConversionTask task);
    Task<ConversionTask> DequeueAsync(CancellationToken cancellationToken);
}

public class ConversionTaskQueue : IConversionTaskQueue
{
    private readonly Queue<ConversionTask> _items = new();
    private readonly SemaphoreSlim _signal = new(0);

    public void QueueBackgroundWorkItem(ConversionTask task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        lock (_items)
        {
            _items.Enqueue(task);
        }

        _signal.Release();
    }

    public async Task<ConversionTask> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);

        lock (_items)
        {
            if (_items.TryDequeue(out var item))
            {
                return item;
            }
        }

        throw new InvalidOperationException("Queue is empty");
    }
}
