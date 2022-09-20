using Azure.Core;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;


namespace metrics
{
    public class ComputeMeter: MeterHelper<long>
    {
       static public string MeterName = "github.com/KnicKnic/azure-metrics/ComputerPageMeter";
       public ComputeMeter(ILogger logger, AzureContext context) :
            base(
            logger,
            MeterName,
            "compute-page",
            "The usage of cores",
            "The limit of cores",
            (SubscriptionResource subscription, AzureLocation location) => new ComputeQuota(subscription, location),
            context)
        {
        }
    }
}
