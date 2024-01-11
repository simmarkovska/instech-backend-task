using Audit.Auditing.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;

namespace Audit.Auditing
{
    public class AuditContext : DbContext, IAuditContext
    {
        public AuditContext(DbContextOptions<AuditContext> options) : base(options)
        {
        }
        public DbSet<ClaimAudit> ClaimAudits { get; set; }
        async Task IAuditContext.AddAsync<T>(T entity, CancellationToken cancellationToken) where T : class
        {
            await Set<T>().AddAsync(entity, cancellationToken);
        }
    }
}
