namespace InvoiceGenerator.Api.Contracts;

public sealed record InvoiceGenerationJob()
{
    public Guid Id { get; } = Guid.CreateVersion7();
}
