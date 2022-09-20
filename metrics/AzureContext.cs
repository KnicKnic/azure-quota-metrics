using Azure.Core;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace metrics
{
    public class AzureContext
    {
        private SubscriptionResource[] subscriptions;
        private AzureLocation location;

        public SubscriptionResource[] Subscriptions{ get => subscriptions;}

        public AzureLocation[] Locations { get
            {
                return new AzureLocation[1]{ location};
            } }

        public AzureContext(SubscriptionResource[] subscriptions, AzureLocation location)
        {
            this.subscriptions = subscriptions;
            this.location = location;
        }
    }
}
