using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InvoiceGenerator.Api.Contracts;
using InvoiceGenerator.Api.Exceptions;
using InvoiceGenerator.Api.Interfaces;

namespace InvoiceGenerator.Api.Services;

public sealed class AzureBlobStorage(BlobContainerClient blobContainerClient) : IFileStorage
{
    public async Task UploadAsync(Guid jobId, FileData fileData, CancellationToken cancellationToken)
    {
        var blobClient = blobContainerClient.GetBlobClient(fileData.FileName);

        var tags = new Dictionary<string, string>
        {
            [InvoiceGenerationJob.Key] = jobId.ToString(),
        };

        await blobClient.UploadAsync(
            BinaryData.FromBytes(fileData.Content),
            new BlobUploadOptions { Tags = tags },
            cancellationToken);
    }

    public async Task<FileData> DownloadAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var blobItems = await blobContainerClient
            .FindBlobsByTagsAsync($"{InvoiceGenerationJob.Key}='{jobId}'", cancellationToken)
            .ToListAsync(cancellationToken);

        if (blobItems.Count == 0)
        {
            throw new InvoiceNotFoundException(jobId);
        }

        if (blobItems.Count > 1)
        {
            throw new DuplicateInvoiceException(jobId, blobItems.Count);
        }

        var blobName = blobItems.Single().BlobName;

        var blobClient = blobContainerClient.GetBlobClient(blobName);

        var download = await blobClient.DownloadContentAsync(cancellationToken);

        var invoiceData = download.Value.Content.ToArray();

        return new FileData
        {
            FileName = blobName,
            Content = invoiceData,
            Type = download.Value.Details.ContentType,
        };
    }
}
