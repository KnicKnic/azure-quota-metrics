﻿using Azure.Core;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace metrics.Meters
{
    // collects metrics that show on the page of quotas for Compute
    public class RoleAssigment : Helper<long>
    {
        static public string MeterName = Constants.MeterBaseName + "RoleAssignmentPageMeter";
        public RoleAssigment(ILogger logger, AzureContext globalContext) :
             base(
             logger,
             MeterName,
             "role-assignment-page",
             "Amount of Role Assignments used, similar to Role Assignments page in Azure porta",
             "Limit of Role Assignments used, similar to Role Assignments page in Azure porta",
             (subscription, location) => new metrics.Quotas.RoleAssigment(subscription),
             globalContext, // only want one location as we generate this data globally
             true)
        {
        }
    }
}
