using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Channels;
using Azure.Identity;
using Azure.Storage.Blobs;
using HandlebarsDotNet;
using InvoiceGenerator.Api.Contracts;
using InvoiceGenerator.Api.Interfaces;
using InvoiceGenerator.Api.Middleware;
using InvoiceGenerator.Api.Services;
using Microsoft.Extensions.Azure;

namespace InvoiceGenerator.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.MapControllers();
        return app;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<InvoiceFactory>();
        services.AddSingleton<PdfGenerator>();
        services.AddSingleton(_ =>
        {
            var channel = Channel.CreateBounded<InvoiceGenerationJob>(new BoundedChannelOptions(50)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
            return channel;
        });
        services.AddSingleton<IJobQueue<InvoiceGenerationJob>, ChannelQueue>();
        services.AddSingleton<IFileStorage, AzureBlobStorage>();
        services.AddSingleton<ConcurrentDictionary<Guid, InvoiceGenerationStatus>>();
        services.AddHostedService<InvoiceGenerationService>();
        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddClient<BlobContainerClient, BlobClientOptions>((options, sp) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                return new BlobContainerClient(
                    new Uri(config["AzureBlobStorage:Uri"] 
                        ?? throw new InvalidOperationException("Azure Blob Storage URI is not configured")),
                    new DefaultAzureCredential(),
                    options);
            });
        });

        AddHelperServices();

        return services;
    }

    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddHostedService<InvoiceGenerationService>();
        return services;
    }

    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("traceId", context.HttpContext.TraceIdentifier);
            };
        });
        return services;
    }

    private static void AddHelperServices()
    {
        Handlebars.RegisterHelper("formatDate", (context, arguments) =>
        {
            if (arguments[0] is DateOnly date)
            {
                return date.ToString("dd/MM/yyyy");
            }
            return arguments[0]?.ToString() ?? "";
        });

        Handlebars.RegisterHelper("formatCurrency", (context, arguments) =>
        {
            if (arguments[0] is decimal value)
            {
                return value.ToString("C", CultureInfo.CreateSpecificCulture("en-US"));
            }
            return arguments[0]?.ToString() ?? "";
        });

        Handlebars.RegisterHelper("formatNumber", (context, arguments) =>
        {
            if (arguments[0] is decimal value)
            {
                return value.ToString("N2");
            }
            return arguments[0]?.ToString() ?? "";
        });

        Handlebars.RegisterHelper("multiply", (context, arguments) =>
        {
            if (arguments.Length >= 2 && arguments[0] is decimal a && arguments[1] is decimal b)
            {
                return a * b;
            }
            return 0m;
        });
    }
}
