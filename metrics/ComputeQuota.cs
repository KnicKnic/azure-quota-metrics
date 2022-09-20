using Azure.Core;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;


namespace metrics
{
    public class ComputeQuota: IQuota<long>
    {
        private SubscriptionResource _subscription;
        private AzureLocation _location;
        public ComputeQuota(SubscriptionResource subscription, AzureLocation location)
        {
            _subscription = subscription;
            _location = location;
        }

        private IEnumerable<KeyValuePair<string, object?>> Keys(ComputeUsage resource, SubscriptionResource subscriptionResource)
        {
            return new[] { new KeyValuePair<string, object?>("name", resource.Name.Value)
                , new KeyValuePair<string, object?>("localized_name", resource.Name.LocalizedValue)
                , new KeyValuePair<string, object?>("unit", resource.Unit)
                , new KeyValuePair<string, object?>("location", _location.Name)
                , new KeyValuePair<string, object?>("subscription", subscriptionResource.Id.SubscriptionId)
                };
        }

        public IEnumerable<QuotaMeasurement<long>> GetQuotas()
        {
            return _subscription.GetUsages(_location).Select(
                item => new QuotaMeasurement<long>(item.CurrentValue, item.Limit, Keys(item, _subscription).ToArray())
                );
        }
        public static IQuota<long> GetQuotasGenerator(SubscriptionResource subscription, AzureLocation location)
        {
            return new ComputeQuota(subscription, location);
        }
    }
}
