using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IConfiguration cfg, ILogger<ProductsController> logger)
    {
        _cfg = cfg;
        _logger = logger;
    }

    /// <summary>
    /// Retorna lista de produtos ativos (Requisito VB6-a)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            var connString = _cfg.GetConnectionString("DefaultConnection");

            using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            // Buscar apenas produtos ativos
            using var cmd = new NpgsqlCommand(
                "SELECT id, name, price FROM products WHERE active = TRUE ORDER BY name",
                conn);

            var products = new List<object>();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new
                {
                    id = reader.GetInt64(0),
                    name = reader.GetString(1),
                    price = reader.GetDecimal(2)
                });
            }

            _logger.LogInformation("Products list retrieved: {Count} items", products.Count);

            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Retorna produto por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { error = "Invalid product ID" });

            var connString = _cfg.GetConnectionString("DefaultConnection");

            using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, name, price, active FROM products WHERE id = @id",
                conn);
            cmd.Parameters.AddWithValue("id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var product = new
                {
                    id = reader.GetInt64(0),
                    name = reader.GetString(1),
                    price = reader.GetDecimal(2),
                    active = reader.GetBoolean(3)
                };

                return Ok(product);
            }

            return NotFound(new { error = $"Product {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

