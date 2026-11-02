using Microsoft.AspNetCore.Mvc;
using Product.Application.DTOs;
using Product.Application.Interfaces;

namespace Product.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly IMasterService _masterService;

    public BrandsController(IMasterService masterService)
    {
        _masterService = masterService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var brands = await _masterService.GetBrandsAsync();
            return Ok(brands);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching brands", detail = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBrandDto dto)
    {
        try
        {
            var created = await _masterService.CreateBrandAsync(dto);
            return Ok(created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating brand", detail = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _masterService.DeleteBrandAsync(id);
            if (!success)
                return NotFound(new { message = $"Brand with ID {id} not found." });

            return Ok(new { message = "Brand deleted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error deleting brand", detail = ex.Message });
        }
    }
}
