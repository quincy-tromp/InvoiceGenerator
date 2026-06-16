namespace InvoiceGenerator.Api.Exceptions;

public sealed class InvoiceNotFoundException : InvalidOperationException
{
    public InvoiceNotFoundException(Guid jobId)
        : base($"Invoice file not found for job '{jobId}'. Data integrity issue detected.")
    {
        JobId = jobId;
    }

    public Guid JobId { get; }
}
