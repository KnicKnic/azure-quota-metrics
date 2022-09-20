using Azure.Core;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace metrics
{
    // collects metrics that show on the page of quotas for Compute
    public class StorageMeter : MeterHelper<int>
    {
        static public string MeterName = "github.com/KnicKnic/azure-metrics/StoragePageMeter";
        public StorageMeter(ILogger logger, AzureContext context) :
             base(
             logger,
             MeterName,
             "storage-page",
             "The usage of accounts",
             "The limit of accounts",
             (SubscriptionResource subscription, AzureLocation location) => new StorageQuota(subscription, location),
             context)
        {
        }
    }
}
