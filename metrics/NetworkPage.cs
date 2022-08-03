using Azure.Core;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace metrics
{
    // collects metrics that show on the page of quotas for Compute
    public class NetworkPageMeter : Meter
    {
        static private readonly string MeterName = "github.com/KnicKnic/azure-metrics/ComputerPageMeter";


        private readonly ILogger<NetworkPageMeter> _logger;
        private SubscriptionResource[] _subscriptions;
        private AzureLocation _location;
        private LinkedList<ObservableGauge<long>> gauges = new LinkedList<ObservableGauge<long>>();
        public NetworkPageMeter(ILogger<NetworkPageMeter> logger, SubscriptionResource[] subscriptions, AzureLocation location)
            :base(MeterName)
        {
            _logger = logger;
            _subscriptions = subscriptions;
            _location = location;

            gauges.Append(CreateObservableGauge<long>(   name: "network-page-quotas",
                                                        observeValues: GetQuotas,
                                                        description: "The usage of accounts"));
            gauges.Append(CreateObservableGauge<long>(   name: "network-page-limits",
                                                        observeValues: GetQuotaLimits,
                                                        description: "The limit of accounts"));
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

        private IEnumerable<Measurement<long>> GetQuotas()
        {
            _logger.LogInformation("Starting to get Network Quotas");
            foreach (SubscriptionResource subscription in _subscriptions)
            {
                var answers = subscription.GetUsages(_location);
                foreach (var answer in answers)
                {
                    if (answer.CurrentValue != 0)
                    {
                        yield return new Measurement<long>(answer.CurrentValue, Keys(answer, subscription));
                    }
                }
            }
            _logger.LogInformation("Completed getting Network Quotas");
        }
        private IEnumerable<Measurement<long>> GetQuotaLimits()
        {
            _logger.LogInformation("Starting to get Network Quota Limits");
            foreach (SubscriptionResource subscription in _subscriptions)
            {
                var answers = subscription.GetUsages(_location);
                foreach (var answer in answers)
                {
                    if (answer.CurrentValue != 0)
                    {
                        yield return new Measurement<long>(answer.Limit, Keys(answer, subscription));
                    }
                }
            }
            _logger.LogInformation("Completed getting Network Quota Limits");
        }
    }
}
