using Azure.Core;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace metrics.Meters
{
    // collects metrics that show on the page of quotas for Compute
    public class Network : Helper<long>
    {
        static public string MeterName = Constants.MeterBaseName + "NetworkPageMeter";
        public Network(ILogger logger, AzureContext context) :
             base(
             logger,
             MeterName,
             "network-page",
             "Amount of network used, similar to quota page in Azure portal",
             "Limit of network used, similar to quota page in Azure portal",
             (subscription, location) => new metrics.Quotas.Network(subscription, location),
             context)
        {
        }
    }
}
