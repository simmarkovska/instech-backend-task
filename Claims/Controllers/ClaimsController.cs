using Claims.Models;
using Claims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly ILogger<ClaimsController> _logger;
        private readonly IClaimService _claimService;
        private readonly IMessageService _messageService;

        public ClaimsController(ILogger<ClaimsController> logger, 
            IClaimService claimService,
            IMessageService messageService
            )
        {
            _logger = logger;
            _claimService = claimService ??
                throw new ArgumentNullException(nameof(claimService));
            _messageService = messageService;
        }

        [HttpGet]
        public Task<IEnumerable<Claim>> GetAsync()
        {
            return _claimService.GetClaimsAsync();
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync(Claim claim)
        {
            IActionResult validationResponse = await _claimService.DateValidation(claim);
            if (validationResponse is OkResult)
            {
                claim.Id = Guid.NewGuid().ToString();
                await _claimService.AddItemAsync(claim);
                await _messageService.SendMessage(new { ClaimId = claim.Id, Method = "POST" });
       
                return Ok(claim);
            }
            else
            {
                return (ActionResult)validationResponse;
            }
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(string id)
        {
            await _messageService.SendMessage(new {ClaimId = id, Method = "DELETE"});
         
            await _claimService.DeleteItemAsync(id);
        }

        [HttpGet("{id}")]
        public async Task<Claim> GetAsync(string id)
        {
            return await _claimService.GetClaimAsync(id);
        }
    }
}