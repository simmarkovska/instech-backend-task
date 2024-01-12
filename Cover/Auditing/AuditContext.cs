using Covers.Auditing.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Covers.Auditing
{
#pragma warning disable 1591 // Disable warning related to missing XML comments

    public class AuditContext : DbContext, IAuditContext
    {
        public AuditContext(DbContextOptions<AuditContext> options) : base(options)
        {
        }
        public DbSet<CoverAudit> CoverAudits { get; set; }

        async Task IAuditContext.AddAsync<T>(T entity, CancellationToken cancellationToken) where T : class
        {
            await Set<T>().AddAsync(entity, cancellationToken);
        }
    }
}
