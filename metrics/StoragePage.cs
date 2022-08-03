using Azure.Core;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace metrics
{
    // collects metrics that show on the page of quotas for Compute
    public class StoragePageMeter : Meter
    {
        static private readonly string MeterName = "github.com/KnicKnic/azure-metrics/ComputerPageMeter";


        private readonly ILogger<StoragePageMeter> _logger;
        private SubscriptionResource[] _subscriptions;
        private AzureLocation _location;
        private LinkedList<ObservableGauge<long>> gauges = new LinkedList<ObservableGauge<long>>();
        public StoragePageMeter(ILogger<StoragePageMeter> logger, SubscriptionResource[] subscriptions, AzureLocation location)
            :base(MeterName)
        {
            _logger = logger;
            _subscriptions = subscriptions;
            _location = location;

            gauges.Append(CreateObservableGauge<long>(   name: "storage-page-quotas",
                                                        observeValues: GetQuotas,
                                                        description: "The usage of accounts"));
            gauges.Append(CreateObservableGauge<long>(   name: "storage-page-limits",
                                                        observeValues: GetQuotaLimits,
                                                        description: "The limit of accounts"));
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

        private IEnumerable<Measurement<long>> GetQuotas()
        {
            _logger.LogInformation("Starting to get Storage Quotas");
            foreach (SubscriptionResource subscription in _subscriptions)
            {
                var answers = subscription.GetUsagesByLocation(_location);
                foreach (var answer in answers)
                {
                    if (answer.CurrentValue != 0)
                    {
                        yield return new Measurement<long>(answer.CurrentValue.Value, Keys(answer, subscription));
                    }
                }
            }
            _logger.LogInformation("Completed getting Storage Quotas");
        }
        private IEnumerable<Measurement<long>> GetQuotaLimits()
        {
            _logger.LogInformation("Starting to get Storage Quota Limits");
            foreach (SubscriptionResource subscription in _subscriptions)
            {
                var answers = subscription.GetUsagesByLocation(_location);
                foreach (var answer in answers)
                {
                    if (answer.CurrentValue != 0)
                    {
                        yield return new Measurement<long>(answer.Limit.Value, Keys(answer, subscription));
                    }
                }
            }
            _logger.LogInformation("Completed getting Storage Quota Limits");
        }
    }
}
