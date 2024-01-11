using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Audit.Auditing.Interfaces
{
    public interface IAuditContext
    {
        DbSet<ClaimAudit> ClaimAudits { get; set; }
        Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
