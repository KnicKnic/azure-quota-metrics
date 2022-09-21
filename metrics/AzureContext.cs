﻿using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace metrics
{
    public class AzureContext
    {        
        public SubscriptionResource[] Subscriptions{ get ; private set; }

        public AzureLocation[] Locations { get; private set; }

        public AzureContext(SubscriptionResource[] subscriptions, AzureLocation[] locations)
        {
            this.Subscriptions = subscriptions;
            this.Locations = locations;
        }
    }
}
