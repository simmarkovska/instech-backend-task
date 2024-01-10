using Claims.Enums;
using Claims.Models;
using Claims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly ILogger<CoversController> _logger;
    private readonly ICoverService _coverService;

    public CoversController(ILogger<CoversController> logger, ICoverService coverService)
    {
        _logger = logger;
        _coverService = coverService;
    }

    [HttpPost("ComputePremuium")]
    public ActionResult ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType)
    {
        return Ok(_coverService.ComputePremium(startDate, endDate, coverType));
    }

    [HttpGet("GetAll")]
    public async Task<IEnumerable<Cover>> GetAsync()
    {
        return await _coverService.GetCoversAsync();
    }

    [HttpGet("{id}")]
    public async Task<Cover> GetAsync(string id)
    {
        return await _coverService.GetCoverAsync(id);
    }

    [HttpPost("Create")]
    public async Task<ActionResult> CreateAsync(Cover cover)
    {
        IActionResult validationResponse = _coverService.DateValidation(cover);
        if (validationResponse is OkResult)
        {
            cover.Id = Guid.NewGuid().ToString();
            cover.Premium = _coverService.ComputePremium(cover.StartDate, cover.EndDate, cover.Type);
            await _coverService.AddItemAsync(cover);
            return Ok(cover);
        }
        else
        {
            return (ActionResult)validationResponse;
        }
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        await _coverService.DeleteItemAsync(id);
    }
}