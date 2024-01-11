using System.Threading.Tasks;

namespace Audit.Auditing.Interfaces
{
    public interface IAuditer
    {
        Task AuditClaim(string id, string httpRequestType);
    }
}
