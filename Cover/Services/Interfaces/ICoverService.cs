using Covers.Enums;
using Covers.Models;
using Microsoft.AspNetCore.Mvc;

namespace Covers.Services.Interfaces
{
#pragma warning disable 1591 // Disable warning related to missing XML comments

    public interface ICoverService
    {
        Task<IEnumerable<Cover>> GetCoversAsync();
        Task<Cover> GetCoverAsync(string id);
        IActionResult DateValidation(Cover cover);
        Task AddItemAsync(Cover cover);
        Task DeleteItemAsync(string id);
        decimal ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType);
    }
}
