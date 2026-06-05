using InvoiceGenerator.Api;
using InvoiceGenerator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi();
builder.Services.AddAppServices();

var app = builder.Build();

app.UseApi();

await app.RunAsync();
