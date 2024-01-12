namespace Covers.Auditing
{
#pragma warning disable 1591 // Disable warning related to missing XML comments

    public class CoverAudit
    {
        public int Id { get; set; }

        public string? CoverId { get; set; }

        public DateTime Created { get; set; }

        public string? HttpRequestType { get; set; }
    }
}
