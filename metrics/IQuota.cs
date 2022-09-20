using Azure.Core;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;


namespace metrics
{
    public interface IQuota<T> where T : struct
    {
        public IEnumerable<QuotaMeasurement<T>> GetQuotas();
    }
}
