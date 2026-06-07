using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Channels;
using HandlebarsDotNet;
using InvoiceGenerator.Api.Contracts;
using InvoiceGenerator.Api.Interfaces;
using InvoiceGenerator.Api.Services;

namespace InvoiceGenerator.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();
        app.MapControllers();
        return app;
    }

    public static IServiceCollection AddAppServices(this IServiceCollection services)
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
        services.AddSingleton<ConcurrentDictionary<Guid, InvoiceGenerationStatus>>();
        services.AddHostedService<InvoiceGenerationService>();
        

        AddHelperServices();

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
