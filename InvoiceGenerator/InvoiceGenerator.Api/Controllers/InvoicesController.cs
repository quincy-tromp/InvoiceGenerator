using InvoiceGenerator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceGenerator.Api.Controllers;

[ApiController]
[Route("invoices")]
public sealed class InvoicesController(
    InvoiceFactory invoiceFactory,
    PdfGenerator pdfGenerator) : ControllerBase
{

    [HttpPost("generate")]
    [ProducesResponseType<IActionResult>(StatusCodes.Status200OK)]
    public async Task<IResult> Generate()
    {
        var invoice = invoiceFactory.Create();
        var pdfData = await pdfGenerator.GenerateAsync(invoice);
        return Results.File(pdfData, "application/pdf", $"invoice-{invoice.Number}.pdf");
    }
}
