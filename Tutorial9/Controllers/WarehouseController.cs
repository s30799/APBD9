using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial9.Model.DTOs;
using Tutorial9.Services;

namespace Tutorial9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        public readonly IDbService dbService;
        private readonly IConfiguration _configuration;

        public WarehouseController(IDbService dbService)
        {
            this.dbService = dbService;
        }

        [HttpPost("addProduct")]
        public async Task<IActionResult> AddProductToWarehouse([FromBody] ProductWarehouseRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "Please provide an amount of Amount" });
            }

            try
            {
                bool orderExists =
                    await dbService.OrderExistsAsync(request.ProductId, request.Amount, request.CreatedAt);
                if (!orderExists)
                {
                    return BadRequest(new
                    {
                        message =
                            "No matching order found for this product and amount, or the order date is not earlier than the request date."
                    });
                }

                bool warehouseExists = await dbService.WarehouseExistsAsync(request.WarehouseId);
                if (!warehouseExists)
                {
                    return BadRequest(new { message = "Warehouse does not exist." });
                }

                bool isOrderAlreadyFulfilled = await dbService.IsOrderFulfilledAsync(request.OrderId);
                if (isOrderAlreadyFulfilled)
                {
                    return BadRequest(new { message = "The order is already fulfilled." });
                }

                int productWarehouseId = await dbService.AddProductToWarehouse(request.ProductId, request.WarehouseId,
                    request.Amount, request.CreatedAt, request.OrderId);

                return Ok(new
                    { message = "Product added successfully to warehouse and order updated", productWarehouseId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to add product: {ex.Message}" });
            }
        }

        [HttpPost("addProductUsingProcedure")]
        public async Task<IActionResult> AddProductUsingProcedure([FromBody] ProductWarehouseRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "Please provide an amount of Amount" });
            }

            try
            {
                bool oderExists =
                    await dbService.OrderExistsAsync(request.ProductId, request.Amount, request.CreatedAt);
                if (!oderExists)
                {
                    return BadRequest(new { message = "Order does not exist." });
                }

                bool warehouseExists = await dbService.WarehouseExistsAsync(request.WarehouseId);
                if (!warehouseExists)
                {
                    return BadRequest(new { message = "Warehouse does not exist." });
                }

                bool isOrderFulfilled = await dbService.IsOrderFulfilledAsync(request.OrderId);
                if (isOrderFulfilled)
                {
                    return BadRequest(new { message = "The order is already fulfilled." });
                }

                int productWarehouseId = await dbService.AddProductToWarehouseUsingProcedure(request.ProductId, request.WarehouseId,
                    request.Amount, request.CreatedAt, request.OrderId);
                return Ok(new { message = "Product added successfully to warehouse and order updated", productWarehouseId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to add product: {ex.Message}" });
            }
        }
    }
   
}
