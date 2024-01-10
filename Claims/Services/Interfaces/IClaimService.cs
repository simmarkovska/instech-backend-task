using Claims.Models;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Services.Interfaces
{
    public interface IClaimService
    {
        Task<IEnumerable<Claim>> GetClaimsAsync();
        Task<Claim> GetClaimAsync(string id);
        Task<IActionResult> DateValidation(Claim claim);
        Task AddItemAsync(Claim claim);
        Task DeleteItemAsync(string id);
    }
}
