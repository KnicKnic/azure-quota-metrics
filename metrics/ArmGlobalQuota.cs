using Azure.Core;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Rest.Azure.OData;
using Microsoft.Rest.Azure;
using System.Threading;

namespace metrics
{
    // https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/azure-subscription-service-limits
    public class ArmGlobalQuota : IQuota<long>
    {

        private SubscriptionResource _subscription;
        private AzureLocation _location;
        private Tuple<string, int>[] _limits;
        public ArmGlobalQuota(SubscriptionResource subscription, AzureLocation location, Tuple<string, int>[] limits)
        {
            _subscription = subscription;
            _location = location;
            _limits = limits;
        }

        private IEnumerable<KeyValuePair<string, object?>> Keys(string name, SubscriptionResource subscriptionResource)
        {
            return new[] { new KeyValuePair<string, object?>("name", name)
                , new KeyValuePair<string, object?>("localized_name", name)
                , new KeyValuePair<string, object?>("unit","count")
                , new KeyValuePair<string, object?>("location", "global")
                , new KeyValuePair<string, object?>("subscription", subscriptionResource.Id.SubscriptionId)
                };
        }

        public IEnumerable<QuotaMeasurement<long>> GetQuotas()
        {
            foreach(var limit in _limits)
            {
                string resourceType = limit.Item1;
                long limits = limit.Item2;
                //long count = 0;
                //var genericResources = _subscription.GetGenericResources($"resourceType eq '{resourceType}'");
                //foreach(var item in genericResources)
                //{
                //    count += 1;
                //    Console.WriteLine(item);
                //}
                long count = _subscription.GetGenericResources($"resourceType eq '{resourceType}'").Count();

                yield return new QuotaMeasurement<long>(count, limits, Keys(resourceType, _subscription).ToArray());
            }
        }
    }
}
