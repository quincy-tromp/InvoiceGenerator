namespace InvoiceGenerator.Api.Contracts;

public sealed record InvoiceGenerationJob()
{
    public const string Key = nameof(InvoiceGenerationJob);

    public Guid Id { get; } = Guid.CreateVersion7();
}
