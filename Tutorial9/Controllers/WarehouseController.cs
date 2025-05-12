using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial9.DTOs;
using Tutorial9.Services;

namespace Tutorial9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }
    
    // Endpoint 1: bez użycia procedury
    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] ProductWarehouseDto productWarehouseDto)
    {
        try
        {
            var result = await _warehouseService.AddProductToWarehouse(productWarehouseDto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occured: "+ ex.Message);
        }
    }
    
    // Endpoint 2: procedura składowana
    [HttpPost("procedure")]
    public async Task<IActionResult> AddProductToWarehouseProc([FromBody] ProductWarehouseDto productWarehouseDto)
    {
        try{
            var result = await _warehouseService.AddProductToWarehouseProc(productWarehouseDto);
            return Ok(result);
        }
        catch (SqlException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occured: "+ ex.Message);
        }
    }
}