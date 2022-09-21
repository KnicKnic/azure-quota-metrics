using Azure.Core;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace metrics
{
    // collects metrics that show on the page of quotas for Compute
    public class RoleAssigmentMeter : MeterHelper<long>
    {
        static public string MeterName = Constants.MeterBaseName + "RoleAssignmentPageMeter";
        public RoleAssigmentMeter(ILogger logger, AzureContext globalContext) :
             base(
             logger,
             MeterName,
             "role-assignment-page",
             "The usage of Role Assignments",
             "The limit of Role Assignments",
             (SubscriptionResource subscription, AzureLocation location) => new RoleAssigmentQuota(subscription),
             globalContext) // only want one location as we generate this data globally
        {
        }
    }
}
