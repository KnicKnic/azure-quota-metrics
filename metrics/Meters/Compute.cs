using Azure.Core;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;


namespace metrics.Meters
{
    public class Compute : Helper<long>
    {
        static public string MeterName = Constants.MeterBaseName + "ComputerPageMeter";
        public Compute(ILogger logger, AzureContext context) :
             base(
             logger,
             MeterName,
             "compute-page",
             "Amount of cpu used, similar to quota page in Azure portal",
             "Limits for CPU used, similar to quota page in Azure portal",
             (subscription, location) => new metrics.Quotas.Compute(subscription, location),
             context)
        {
        }
    }
}
