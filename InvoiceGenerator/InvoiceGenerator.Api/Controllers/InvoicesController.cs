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
    public IActionResult Generate()
    {
        var invoice = invoiceFactory.Create();
        var html = pdfGenerator.Generate(invoice);
        return Ok(html);
    }
}
