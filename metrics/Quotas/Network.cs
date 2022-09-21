using Azure.Core;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace metrics.Quotas
{
    // collects metrics that show on the page of quotas for Compute
    public class Network : IQuota<long>
    {
        private SubscriptionResource _subscription;
        private AzureLocation _location;
        public Network(SubscriptionResource subscription, AzureLocation location)
        {
            _subscription = subscription;
            _location = location;
        }

        IEnumerable<KeyValuePair<string, object?>> Keys(NetworkUsage resource, SubscriptionResource subscriptionResource)
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
    }
}
