using System.Runtime.InteropServices;
using McMaster.Extensions.CommandLineUtils;
using System.Diagnostics.Metrics;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Reservations;
using Microsoft.Azure.Management.Quota;
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
using Microsoft.Azure.Management.Authorization;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.Rest;
using metrics.Meters;

public class Program
{
    [Required]
    [Option(Description = "Required. The Subscription id(s).\nEx: '--Subscription abcdef01-0123-0123-0123-0123456789ab'.\nCan be specified more than once for multiple values")]
    public string[] Subscription { get; set; }

    [Required]
    [Option(Description ="Required. The region name(s).\nEx: '--Location EastUS'.\nCan be specified more than once for multiple values")]
    public string[] Location { get; set; }

    // should be in form of
    // Microsoft.Network/trafficManagerProfiles=200
    [Option(Description = "Optional. Additional ARM type(s) to monitor <ArmType=LimitValue>.\nEx: '--ArmLimit Microsoft.Network/trafficManagerProfiles=200'.\nCan be specified more than once for multiple values")]
    public string[] ArmLimit { get; set; }

    public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

    public void OnExecute()
    {
        // https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/azure-services-resource-providers
        // https://docs.microsoft.com/en-us/rest/api/reserved-vm-instances/quotaapi
        // apis https://docs.microsoft.com/en-us/dotnet/azure/sdk/packages

        // See https://aka.ms/new-console-template for more information

        // how do I auto register resource provider?

        Console.WriteLine("Starting");

        var credential = new DefaultAzureCredential();
        ArmClient client = new ArmClient(credential);

        var subscriptions = this.Subscription.Select(subscription => client.GetSubscriptionResource(new Azure.Core.ResourceIdentifier("/subscriptions/" + subscription))).ToArray();

        var location = this.Location.Select(l => new Azure.Core.AzureLocation(l.Trim())).ToArray();

        // should we validate the locations?
        //// this doesn't work for non public azure regions
        //foreach (var loc in location)
        //{
        //    if ((loc.DisplayName ?? "") == "")
        //    {
        //        throw new Exception("string is empty");
        //    }
        //}


        var loggerFactory = new LoggerConfiguration().WriteTo.Console();

        using var log = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        
        



        var builder = WebApplication.CreateBuilder();

        // process arm limits of form Microsoft.Network/trafficManagerProfiles=200
        var armLimitsParsed = ArmLimit.Select(limit => new Tuple<string, int>(limit.Split("=")[0], int.Parse(limit.Split("=")[1]))).ToArray();
        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        //builder.Services.AddSingleton(ComputePageMeter)
        var ilogger = new SerilogLoggerFactory(log).CreateLogger<Compute>();
        var azureContext = new AzureContext(subscriptions, location);
        var globalAzureContext = new AzureContext(subscriptions, location.Take(1).ToArray()); // only want one location as we won't use it

        builder.Services.AddSingleton(azureContext);

        using var computePageMeter = new Compute(new SerilogLoggerFactory(log).CreateLogger<Compute>(), azureContext);
        using var storagePageMeter = new StorageMeter(new SerilogLoggerFactory(log).CreateLogger<StorageMeter>(), azureContext);
        using var networkPageMeter = new Network(new SerilogLoggerFactory(log).CreateLogger<Network>(), azureContext);
        using var armPageMeter = new ArmGlobal(new SerilogLoggerFactory(log).CreateLogger<ArmGlobal>(), globalAzureContext, armLimitsParsed);
        using var roleAssigmentPageMeter = new RoleAssigment(new SerilogLoggerFactory(log).CreateLogger<RoleAssigment>(), globalAzureContext);

        using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter(computePageMeter.Name)
                .AddMeter(storagePageMeter.Name)
                .AddMeter(networkPageMeter.Name)
                .AddMeter(armPageMeter.Name)
                .AddMeter(roleAssigmentPageMeter.Name)
                .AddPrometheusExporter()
                .Build();

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

        app.MapControllers();

        Console.WriteLine("Going into run loop");
        app.Run();
    }
}