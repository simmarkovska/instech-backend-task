using Covers.Models;
using Covers.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Covers.Controllers;

///<summary>
///CoversController
///</summary>
[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly ILogger<CoversController> _logger;
    private readonly ICoverService _coverService;

    ///<summary>
    ///CoversController constructor
    ///</summary>
    public CoversController(ILogger<CoversController> logger, ICoverService coverService)
    {
        _logger = logger;
        _coverService = coverService;
    }

    ///<summary>
    ///Retrieve all Covers
    ///</summary>
    [HttpGet]
    public async Task<ActionResult> GetAsync()
    {
        try
        {
            var covers = await _coverService.GetCoversAsync();
            return Ok(covers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500, "An error occurred while fetching covers.");
        }
    }

    ///<summary>
    ///Create new Cover
    ///</summary>
    [HttpPost]
    public async Task<ActionResult> CreateAsync(Cover cover)
    {
        try
        {
            IActionResult validationResponse = _coverService.DateValidation(cover);
            if (validationResponse is OkResult)
            {
                cover.Premium = _coverService.ComputePremium(cover.StartDate, cover.EndDate, cover.Type);
                await _coverService.AddItemAsync(cover);
                return Ok(cover);
            }
            else
            {
                return (ActionResult)validationResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500, "An error occurred while creating a cover.");
        }
    }

    ///<summary>
    ///Retrieve Cover by unique Cover Id
    ///</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetAsync(string id)
    {
        try
        {
            var cover = await _coverService.GetCoverAsync(id);
            if (cover == null)
            {
                return NotFound();
            }

            return Ok(cover);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500, "An error occurred while fetching cover.");
        }
    }

    ///<summary>
    ///Delete Cover by unique Cover Id
    ///</summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(string id)
    {
        try
        {
            await _coverService.DeleteItemAsync(id);
            return NoContent(); // Return 204 No Content if sucessfully deleted
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500, "An error occurred while deleting cover.");
        }
    }
}