using InvoiceGenerator.Api;
using InvoiceGenerator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices();
builder.Services.AddApplicationServices();
builder.Services.AddBackgroundJobs();
builder.Services.AddErrorHandling();

var app = builder.Build();

app.UseApiServices();
app.UseHttpsRedirection();
app.UseExceptionHandler();

await app.RunAsync();
