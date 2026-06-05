using HandlebarsDotNet;
using InvoiceGenerator.Api.Contracts;

namespace InvoiceGenerator.Api.Services;

public sealed class PdfGenerator()
{
    public string? Generate(Invoice invoice)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "InvoiceTemplate.hbs");
        var templateContent = File.ReadAllText(templatePath);

        var template = Handlebars.Compile(templateContent);

        var data = new
        {
            invoice.Number,
            invoice.IssuedDate,
            invoice.DueDate,
            invoice.SellerAddress,
            invoice.CustomerAddress,
            invoice.LineItems,
            Total = invoice.LineItems.Sum(li => li.Quantity * li.Price)
        };

        var html = template(data);

        return html;
    }
}
