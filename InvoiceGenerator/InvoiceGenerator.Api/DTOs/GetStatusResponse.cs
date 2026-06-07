namespace InvoiceGenerator.Api.DTOs;

public sealed record GetStatusResponse
{
    public required Guid Id { get; init; }
    public required string Status { get; init; } 
    public required string Link { get; init; } 
}
