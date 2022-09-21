using Azure.Core;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using Azure.ResourceManager.Network.Models;
using Microsoft.Azure.Management.Authorization;
using Azure.ResourceManager;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Microsoft.Rest;
using Azure.Identity;

namespace metrics.Quotas
{
    // collects metrics that show on the page of quotas for Compute
    public class RoleAssigment : IQuota<long>
    {
        private SubscriptionResource _subscription;
        public RoleAssigment(SubscriptionResource subscription)
        {
            _subscription = subscription;
        }

        IEnumerable<KeyValuePair<string, object?>> Keys(SubscriptionResource subscriptionResource)
        {
            return new[] { new KeyValuePair<string, object?>("name", "RoleAssigments")
                , new KeyValuePair<string, object?>("localized_name","Role Assigments")
                , new KeyValuePair<string, object?>("unit","count")
                , new KeyValuePair<string, object?>("location", "global")
                , new KeyValuePair<string, object?>("subscription", subscriptionResource.Id.SubscriptionId)
            };
        }
        public IEnumerable<QuotaMeasurement<long>> GetQuotas()
        {
            var metrics = GetClientHack().RoleAssignmentMetrics.GetMetricsForSubscription();
            yield return new QuotaMeasurement<long>(metrics.RoleAssignmentsCurrentCount ?? 0, metrics.RoleAssignmentsLimit ?? 0, Keys(_subscription).ToArray());
        }

        private AuthorizationManagementClient GetClientHack()
        {// should figure out better way to construct this token
            var credential = new DefaultAzureCredential();
            var token = credential.GetToken(new TokenRequestContext(new[] { "https://management.core.windows.net/.default" })).Token;

            ServiceClientCredentials creds = new TokenCredentials(token);
            return new AuthorizationManagementClient(creds) { SubscriptionId = _subscription.Id.SubscriptionId };
        }
    }
}
