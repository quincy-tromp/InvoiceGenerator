using System.Threading.Channels;
using InvoiceGenerator.Api.Contracts;
using InvoiceGenerator.Api.Interfaces;

namespace InvoiceGenerator.Api.Services;

public sealed class ChannelQueue(Channel<InvoiceGenerationJob> channel) : IJobQueue<InvoiceGenerationJob>
{
    public async Task EnqueueAsync(InvoiceGenerationJob job, CancellationToken cancellationToken = default)
    {
        await channel.Writer.WriteAsync(job, cancellationToken);
    }
}
