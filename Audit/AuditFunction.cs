using System.Text.Json;
using System.Threading.Tasks;
using Audit.Auditing;
using Audit.Auditing.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Audit
{
    public class AuditFunction
    {
        private readonly IAuditer _auditer;
        public AuditFunction(IAuditContext auditContext) {
            _auditer = new Auditer(auditContext);
        }

        [FunctionName("AuditFunction")]
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
