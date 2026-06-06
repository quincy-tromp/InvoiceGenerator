using HandlebarsDotNet;
using InvoiceGenerator.Api.Contracts;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace InvoiceGenerator.Api.Services;

public sealed class PdfGenerator()
{
    public async Task<byte[]> GenerateAsync(Invoice invoice)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "InvoiceTemplate.hbs");
        var templateContent = await File.ReadAllTextAsync(templatePath);

        var template = Handlebars.Compile(templateContent);

        var data = new
        {
            invoice.Number,
            invoice.IssuedDate,
            invoice.DueDate,
            invoice.SellerAddress,
            invoice.CustomerAddress,
            invoice.LineItems,
            SubTotal = invoice.LineItems.Sum(li => li.Quantity * li.Price),
            Total = invoice.LineItems.Sum(li => li.Quantity * li.Price)
        };

        var html = template(data);

        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });

        using var page = await browser.NewPageAsync();

        await page.SetContentAsync(html);

        await page.EvaluateExpressionHandleAsync("document.fonts.ready");

        var pdfData = await page.PdfDataAsync(new PdfOptions
        {
            HeaderTemplate =
            """
            <div style='font-size: 14px; text-align: center; padding: 10px;'>
                <span style='margin-right: 20px;'><span class='title'></span></span>
                <span><span class='date'></span></span>
            </div>
            """,
            FooterTemplate =
            """
            <div style='font-size: 14px; text-align: center; padding: 10px;'>
                <span style='margin-right: 20px;'>Generated on <span class='date'></span></span>
                <span>Page <span class='pageNumber'></span> of <span class='totalPages'></span></span>
            </div>
            """,
            DisplayHeaderFooter = true,
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "50px",
                Right = "20px",
                Bottom = "50px",
                Left = "20px"
            }
        });

        return pdfData;
    }
}
