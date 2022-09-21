using Azure.Core;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;
using metrics.Quotas;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace metrics.Meters
{
    // collects metrics that show on the page of quotas for Compute
    public class Helper<T> : Meter where T : struct
    {
        private readonly ILogger _logger;
        private AzureContext _context;
        private LinkedList<ObservableGauge<T>> gauges = new LinkedList<ObservableGauge<T>>();
        private Func<SubscriptionResource, AzureLocation, IQuota<T>> _quotaGenerator;
        private string _name;
        private bool returnEmptyValues;

        private List<QuotaMeasurement<T>> quotaMeasurements = new List<QuotaMeasurement<T>>();

        public Helper(ILogger logger,
                            string MeterName,
                            string name,
                            string descriptionValues,
                            string descriptionLimits,
                            Func<SubscriptionResource, AzureLocation, IQuota<T>> quotaGenerator,
                            AzureContext context,
                            bool returnEmptyValues = false)
            : base(MeterName)
        {
            _logger = logger;
            _context = context;
            _name = name;
            this.returnEmptyValues = returnEmptyValues;
            _quotaGenerator = quotaGenerator;
            gauges.Append(CreateObservableGauge(name: name + "-quotas",
                                                      observeValues: GetQuotas,
                                                      description: descriptionValues));
            gauges.Append(CreateObservableGauge(name: name + "-limits",
                                                      observeValues: GetQuotaLimits,
                                                      description: descriptionLimits));
        }

        private IEnumerable<QuotaMeasurement<T>> GetMeasurements()
        {
            _logger.LogInformation("Starting to get " + _name + "-quotas");
            foreach (SubscriptionResource subscription in _context.Subscriptions)
            {
                foreach (var location in _context.Locations)
                {
                    _logger.LogInformation("Fetching " + _name + "-quotas for " + subscription.Id.SubscriptionId + " in " + location.ToString());
                    var answers = _quotaGenerator(subscription, location).GetQuotas();
                    foreach (var answer in answers)
                    {
                        if (returnEmptyValues || !answer.IsZero)
                        {
                            yield return answer;
                        }
                    }
                }
            }
            _logger.LogInformation("Completed getting " + _name + "-quotas");
        }

        private IEnumerable<Measurement<T>> GetQuotas()
        {
            quotaMeasurements = GetMeasurements().ToList();
            return quotaMeasurements.Select(answer => new Measurement<T>(answer.Value, answer.Keys));
        }
        private IEnumerable<Measurement<T>> GetQuotaLimits()
        {
            return quotaMeasurements.Select(answer => new Measurement<T>(answer.Limit, answer.Keys));
        }
    }
}
