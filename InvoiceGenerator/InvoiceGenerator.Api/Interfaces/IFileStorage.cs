using InvoiceGenerator.Api.Contracts;

namespace InvoiceGenerator.Api.Interfaces;

public interface IFileStorage
{
    Task UploadAsync(Guid jobId, FileData fileData, CancellationToken cancellationToken);
    Task<FileData> DownloadAsync(Guid jobId, CancellationToken cancellationToken);
}
