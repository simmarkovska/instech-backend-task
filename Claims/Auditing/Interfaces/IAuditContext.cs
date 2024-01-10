using Microsoft.EntityFrameworkCore;

namespace Claims.Auditing.Interfaces
{
    public interface IAuditContext
    {
        DbSet<ClaimAudit> ClaimAudits { get; set; }
        int SaveChanges();
        Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
