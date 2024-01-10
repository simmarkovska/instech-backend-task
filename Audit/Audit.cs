using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Claims.Auditing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Audit
{
    public class Audit
    {
        private readonly Auditer _auditer;
        public Audit(AuditContext auditContext) {
            _auditer = new Auditer(auditContext);
        }

        [FunctionName("Audit")]
        public async Task RunAsync([ServiceBusTrigger("audit-claim", Connection = "QueueConnectionString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            JsonDocument jsonDocument = JsonDocument.Parse(myQueueItem);
            JsonElement root = jsonDocument.RootElement;
            string claimId = root.GetProperty("ClaimId").GetString();
            string method = root.GetProperty("Method").GetString();
            await _auditer.AuditClaim(claimId, method);
        }
    }
}
