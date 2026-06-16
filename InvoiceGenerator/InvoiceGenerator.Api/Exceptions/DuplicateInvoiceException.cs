namespace InvoiceGenerator.Api.Exceptions;

public sealed class DuplicateInvoiceException : InvalidOperationException
{
    public DuplicateInvoiceException(Guid jobId, int fileCount)
        : base($"Expected exactly one file for job '{jobId}', but found {fileCount}.")
    {
        JobId = jobId;
        FileCount = fileCount;
    }

    public Guid JobId { get; }
    public int FileCount { get; }
}
