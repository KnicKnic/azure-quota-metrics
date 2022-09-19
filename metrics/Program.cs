using System.Runtime.InteropServices;
using McMaster.Extensions.CommandLineUtils;
using System.Diagnostics.Metrics;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Reservations;
using Microsoft.Azure.Management.Quota;
using McMaster.Extensions.CommandLineUtils;
//using Microsoft.Azure.Management.Reservations;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.Logging;
using metrics;
using Serilog;
using Serilog.Extensions.Logging;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.ComponentModel.DataAnnotations;
using Azure.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

public class Program
{
    [Option]
    public string[] Name { get; set; } = { "localhost", "127.0.0.1" };
    [Option]
    public string Port { get; set; } = "9184";

    [Option]
    public string[] Subscription { get; set; } = { "7c5b2a0d-bcc2-41f7-bcea-c381f49e6d1f" };

    [Option]
    public string Location { get; set; } = "EastUS";

    public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

    public void OnExecute()
    {
        // https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/azure-services-resource-providers
        // https://docs.microsoft.com/en-us/rest/api/reserved-vm-instances/quotaapi
        // apis https://docs.microsoft.com/en-us/dotnet/azure/sdk/packages

        // See https://aka.ms/new-console-template for more information

        // how do I auto register resource provider?

        Console.WriteLine("Hello, World!");

        var credential = new DefaultAzureCredential();

        ArmClient client = new ArmClient(credential);
        var subscriptions = this.Subscription.Select(subscription => client.GetSubscriptionResource(new Azure.Core.ResourceIdentifier("/subscriptions/" + subscription))).ToArray();

        var location = new Azure.Core.AzureLocation(this.Location);


        var loggerFactory = new LoggerConfiguration().WriteTo.Console();

        using var log = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        using var computePageMeter = new ComputePageMeter(new SerilogLoggerFactory(log).CreateLogger<ComputePageMeter>(), subscriptions, location);
        using var storagePageMeter = new StoragePageMeter(new SerilogLoggerFactory(log).CreateLogger<StoragePageMeter>(), subscriptions, location);
        using var NetworkPageMeter = new NetworkPageMeter(new SerilogLoggerFactory(log).CreateLogger<NetworkPageMeter>(), subscriptions, location);

        //foreach( var page in computePageMeter.GetQuotas())
        //{
        //    Console.WriteLine(page.Value);
        //}
        using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter(computePageMeter.Name)
                .AddMeter(storagePageMeter.Name)
                .AddPrometheusExporter(opt =>
                {
                    //opt.StartHttpListener = true;
                    //opt.HttpListenerPrefixes = this.Name.Select(name => $"http://" + name + ":" + this.Port + "/").ToArray();                        //opt.StartHttpListener = true;
                    //opt.HttpListenerPrefixes = this.Name.Select(name => $"http://" + name + ":" + this.Port + "/").ToArray();
                })
                .Build();


        var builder = WebApplication.CreateBuilder();

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton(meterProvider);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        app.UseOpenTelemetryPrometheusScrapingEndpoint();
        app.UseAuthorization();

        //app.UseEndpoints(endpoints =>
        //{
        //    endpoints.MapControllers();
        //});

        app.MapControllers();

        app.Run();
    }
}