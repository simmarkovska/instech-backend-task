namespace Claims.Auditing.Interfaces
{
#pragma warning disable 1591 // Disable warning related to missing XML comments

    public interface IAuditer
    {
        Task AuditCover(string id, string httpRequestType);
    }
}
