using Azure.Core;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace metrics
{
    // collects metrics that show on the page of quotas for Compute
    public class NetworkMeter : MeterHelper<long>
    {
        static public string MeterName = "github.com/KnicKnic/azure-metrics/NetworkPageMeter";
        public NetworkMeter(ILogger logger, AzureContext context) :
             base(
             logger,
             MeterName,
             "network-page",
             "The usage of accounts",
             "The limit of accounts",
             (SubscriptionResource subscription, AzureLocation location) => new NetworkQuota(subscription, location),
             context)
        {
        }
    }
}
