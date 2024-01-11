using Claims.Auditing.Interfaces;

namespace Claims.Auditing
{
#pragma warning disable 1591 // Disable warning related to missing XML comments

    public class Auditer : IAuditer
    {
        private readonly IAuditContext _auditContext;

        public Auditer(IAuditContext auditContext)
        {
            _auditContext = auditContext;
        }


        public async Task AuditCover(string id, string httpRequestType)
        {
            var coverAudit = new CoverAudit()
            {
                Created = DateTime.Now,
                HttpRequestType = httpRequestType,
                CoverId = id
            };

            await _auditContext.AddAsync(coverAudit);
            await _auditContext.SaveChangesAsync();
        }
    }
}
