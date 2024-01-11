using Claims.Models;
using Claims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers
{
    ///<summary>
    ///ClaimsController
    ///</summary>
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly ILogger<ClaimsController> _logger;
        private readonly IClaimService _claimService;
        private readonly IMessageService _messageService;

        ///<summary>
        ///ClaimsController constructor
        ///</summary>
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

        ///<summary>
        ///Retrieve all Claims
        ///</summary>
        [HttpGet]
        public async Task<ActionResult> GetAsync()
        {
            try
            {
                var claims = await _claimService.GetClaimsAsync();
                return Ok(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "An error occurred while fetching claims.");
            }
        }

        ///<summary>
        ///Create new Claim
        ///</summary>
        [HttpPost]
        public async Task<ActionResult> CreateAsync(Claim claim)
        {
            try
            {
                IActionResult validationResponse = await _claimService.DateValidation(claim);

                if (validationResponse is OkResult)
                {
                    await _claimService.AddItemAsync(claim);
                    await _messageService.SendMessage(new { ClaimId = claim.Id, Method = "POST" });

                    return Ok(claim);
                }
                else
                {
                    return (ActionResult)validationResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "An error occurred while creating a claim.");
            }
        }


        ///<summary>
        ///Retrieve Claim by unique Claim Id
        ///</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetAsync(string id)
        {
            try
            {
                var claim = await _claimService.GetClaimAsync(id);
                if (claim == null)
                {
                    return NotFound();
                }

                return Ok(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "An error occurred while fetching claim.");
            }
        }

        ///<summary>
        ///Delete Claim by unique Claim Id
        ///</summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            try
            {
                await _messageService.SendMessage(new { ClaimId = id, Method = "DELETE" });
                await _claimService.DeleteItemAsync(id);

                return NoContent(); // Return 204 No Content if sucessfully deleted
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "An error occurred while deleting claim.");
            }
        }
    }
}