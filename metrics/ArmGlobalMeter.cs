using Azure.Core;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;


namespace metrics
{
    public class ArmGlobalMeter : MeterHelper<long>
    {
        static public string MeterName = Constants.MeterBaseName + "ArmMeter";
        public ArmGlobalMeter(ILogger logger, AzureContext globalContext, Tuple<string, int>[] armLimits) :
             base(
             logger,
             MeterName,
             "arm-page",
             "The usage of arm objects",
             "The limit of arm objects",
             (SubscriptionResource subscription, AzureLocation location) => new ArmGlobalQuota(subscription, location, armLimits),
             globalContext)
        {
        }
    }
}

