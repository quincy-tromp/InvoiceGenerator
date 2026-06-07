using InvoiceGenerator.Api.Contracts;

namespace InvoiceGenerator.Api.Interfaces;

public interface IJobQueue<in T> 
{
    Task EnqueueAsync(T job, CancellationToken cancellationToken = default);
    IAsyncEnumerable<InvoiceGenerationJob> DequeueAllAsync(CancellationToken cancellationToken = default);
}
