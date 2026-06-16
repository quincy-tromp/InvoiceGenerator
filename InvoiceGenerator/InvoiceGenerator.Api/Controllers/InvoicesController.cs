using System.Collections.Concurrent;
using System.Net.Mime;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using InvoiceGenerator.Api.Contracts;
using InvoiceGenerator.Api.DTOs;
using InvoiceGenerator.Api.Interfaces;
using InvoiceGenerator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceGenerator.Api.Controllers;

[ApiController]
[Route("invoices")]
public sealed class InvoicesController(
    IJobQueue<InvoiceGenerationJob> jobQueue,
    IFileStorage storageService,
    ConcurrentDictionary<Guid, InvoiceGenerationStatus> statusDiectionary,
    LinkGenerator linkGenerator
    ) : ControllerBase
{

    [HttpPost("generate")]
    [ProducesResponseType<ActionResult<GetStatusResponse>>(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Generate()
    {
        var job = new InvoiceGenerationJob();
        await jobQueue.EnqueueAsync(job);

        statusDiectionary[job.Id] = InvoiceGenerationStatus.Queued;

        var statusUrl = GetLink(nameof(GetStatus), new { id = job.Id });

        var response = new GetStatusResponse
        {
            Id = job.Id,
            Status = InvoiceGenerationStatus.Queued.ToString(),
            Link = statusUrl
        };

        return Accepted(response);
    }

    [HttpGet("{id:guid}/status")]
    [ProducesResponseType<ActionResult<GetStatusResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ActionResult>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetStatusResponse>> GetStatus(Guid id)
    {
        if (!statusDiectionary.TryGetValue(id, out var status))
        {
            return NotFound();
        }

        var response = new GetStatusResponse
        {
            Id = id,
            Status = status.ToString(),
        };

        if (status == InvoiceGenerationStatus.Completed)
        {
            var invoiceUrl = GetLink(nameof(GetInvoice), new { id });
            response = response with { Link = invoiceUrl };
        }

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ActionResult<byte[]>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ActionResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ActionResult>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInvoice(Guid id, CancellationToken cancellationToken = default)
    {
        if (!statusDiectionary.TryGetValue(id, out var status))
        {
            return NotFound();
        }

        if (status != InvoiceGenerationStatus.Completed)
        {
            return BadRequest("Invoice generation is not completed yet.");
        }

        var fileData = await storageService.DownloadAsync(id, cancellationToken);

        return File(fileData.Content, MediaTypeNames.Application.Pdf, fileData.FileName);
    }

    private string GetLink(string actionName, object values)
    {
        return linkGenerator.GetUriByAction(
            HttpContext,
            action: actionName,
            controller: "Invoices",
            values) ?? throw new Exception("Failed to generate link.");
    }
}
