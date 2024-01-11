using System;
using System.Threading.Tasks;
using Audit.Auditing.Interfaces;

namespace Audit.Auditing
{
    public class Auditer : IAuditer
    {
        private readonly IAuditContext _auditContext;

        public Auditer(IAuditContext auditContext)
        {
            _auditContext = auditContext;
        }

        public async Task AuditClaim(string id, string httpRequestType)
        {
            var claimAudit = new ClaimAudit()
            {
                Created = DateTime.Now,
                HttpRequestType = httpRequestType,
                ClaimId = id
            };

            await _auditContext.AddAsync(claimAudit);
            await _auditContext.SaveChangesAsync();
        }
    }
}
