using System.Collections.Concurrent;
using InvoiceGenerator.Api.Contracts;
using InvoiceGenerator.Api.Interfaces;

namespace InvoiceGenerator.Api.Services;

public sealed class InvoiceGenerationService(
    ILogger<InvoiceGenerationService> logger,
    IJobQueue<InvoiceGenerationJob> jobQueue,
    ConcurrentDictionary<Guid, InvoiceGenerationStatus> statusDictionary) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in jobQueue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await ProcessJobAsync(job, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing invoice generation job {JobId}", job.Id);
            }
        }
    }

    private async Task ProcessJobAsync(InvoiceGenerationJob job, CancellationToken cancellationToken)
    {
        try
        {
            statusDictionary[job.Id] = InvoiceGenerationStatus.Processing;

            // Simulate invoice generation time
            await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            statusDictionary[job.Id] = InvoiceGenerationStatus.Completed;
        }
        catch (Exception)
        {
            statusDictionary[job.Id] = InvoiceGenerationStatus.Failed;
            throw;
        }
    }
}
