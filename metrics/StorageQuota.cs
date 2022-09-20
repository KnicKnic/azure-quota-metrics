using Azure.Core;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using Azure.ResourceManager.Network.Models;

namespace metrics
{
    // collects metrics that show on the page of quotas for Compute
    public class StorageQuota : IQuota<int>
    {
        private SubscriptionResource _subscription;
        private AzureLocation _location;
        public StorageQuota(SubscriptionResource subscription, AzureLocation location)
        {
            _subscription = subscription;
            _location = location;
        }

        IEnumerable<KeyValuePair<string, object?>> Keys(StorageUsage resource, SubscriptionResource subscriptionResource)
        {
            return new[] { new KeyValuePair<string, object?>("name", resource.Name.Value)
                , new KeyValuePair<string, object?>("localized_name", resource.Name.LocalizedValue)
                , new KeyValuePair<string, object?>("unit", resource.Unit)
                , new KeyValuePair<string, object?>("location", _location.Name)
                , new KeyValuePair<string, object?>("subscription", subscriptionResource.Id.SubscriptionId)
                };
        }
        public IEnumerable<QuotaMeasurement<int>> GetQuotas()
        {
            return _subscription.GetUsagesByLocation(_location).Select(
                item => new QuotaMeasurement<int>(item.CurrentValue??0, item.Limit??0, Keys(item, _subscription).ToArray())
                );
        }
    }
}
