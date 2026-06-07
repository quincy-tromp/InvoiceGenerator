namespace InvoiceGenerator.Api.Contracts;

public enum InvoiceGenerationStatus
{
    Queued,
    Processing,
    Completed,
    Failed
}
