namespace InvoiceGenerator.Api.Contracts;

public sealed class FileData
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
    public string Type { get; set; }
}
