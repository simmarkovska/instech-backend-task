using Claims.Auditing.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Claims.Auditing
{
    public class AuditContext : DbContext, IAuditContext
    {
        public AuditContext(DbContextOptions<AuditContext> options) : base(options)
        {
        }
        public DbSet<ClaimAudit> ClaimAudits { get; set; }
        public DbSet<CoverAudit> CoverAudits { get; set; }

        async Task IAuditContext.AddAsync<T>(T entity, CancellationToken cancellationToken) where T : class
        {
            await Set<T>().AddAsync(entity, cancellationToken);
        }
    }
}
