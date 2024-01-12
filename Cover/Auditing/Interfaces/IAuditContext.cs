using Microsoft.EntityFrameworkCore;

namespace Covers.Auditing.Interfaces
{
#pragma warning disable 1591 // Disable warning related to missing XML comments
    public interface IAuditContext
    {
        DbSet<CoverAudit> CoverAudits { get; set; }
        Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
