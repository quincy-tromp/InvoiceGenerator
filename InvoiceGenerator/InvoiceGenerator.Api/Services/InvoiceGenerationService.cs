using System.Collections.Concurrent;
using System.Net.Mime;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InvoiceGenerator.Api.Contracts;
using InvoiceGenerator.Api.Interfaces;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace InvoiceGenerator.Api.Services;

public sealed class InvoiceGenerationService(
    ILogger<InvoiceGenerationService> logger,
    IJobQueue<InvoiceGenerationJob> jobQueue,
    ConcurrentDictionary<Guid, InvoiceGenerationStatus> statusDictionary,
    InvoiceFactory invoiceFactory,
    PdfGenerator pdfGenerator,
    IFileStorage storageService
) : BackgroundService
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

            var invoice = await invoiceFactory.CreateAsync(cancellationToken);

            var invoicePdf = await pdfGenerator.GenerateAsync(invoice, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var fileData = new FileData
            {
                FileName = invoice.GetFileName(),
                Content = invoicePdf,
                Type = MediaTypeNames.Application.Pdf
            };
            await storageService.UploadAsync(job.Id, fileData, cancellationToken);

            statusDictionary[job.Id] = InvoiceGenerationStatus.Completed;
        }
        catch (Exception)
        {
            statusDictionary[job.Id] = InvoiceGenerationStatus.Failed;
            throw;
        }
    }
}
