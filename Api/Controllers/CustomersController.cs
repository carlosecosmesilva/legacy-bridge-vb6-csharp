using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(IConfiguration cfg, ILogger<CustomersController> logger)
    {
        _cfg = cfg;
        _logger = logger;
    }

    /// <summary>
    /// Busca clientes por nome (ILIKE case-insensitive) - Requisitos VB6-b e C#-b
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string term,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        try
        {
            if (limit <= 0 || limit > 1000)
                return BadRequest(new { error = "Limit must be between 1 and 1000" });

            if (offset < 0)
                return BadRequest(new { error = "Offset must be >= 0" });

            var connString = _cfg.GetConnectionString("DefaultConnection");

            using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            // Chama a função PostgreSQL que usa ILIKE
            using var cmd = new NpgsqlCommand(
                "SELECT * FROM search_customers_by_name(@term, @limit, @offset)",
                conn);
            cmd.Parameters.AddWithValue("term", term ?? string.Empty);
            cmd.Parameters.AddWithValue("limit", limit);
            cmd.Parameters.AddWithValue("offset", offset);

            var customers = new List<object>();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                customers.Add(new
                {
                    id = reader.GetInt64(0),
                    name = reader.GetString(1),
                    document = reader.IsDBNull(2) ? null : reader.GetString(2),
                    status = reader.GetString(3)
                });
            }

            _logger.LogInformation(
                "Customer search executed: Term='{Term}', ResultCount={Count}",
                term, customers.Count);

            return Ok(new
            {
                data = customers,
                pagination = new
                {
                    limit,
                    offset,
                    count = customers.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers with term: {Term}", term);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Retorna cliente por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            if (id <= 0)
                return BadRequest(new { error = "Invalid customer ID" });

            var connString = _cfg.GetConnectionString("DefaultConnection");

            using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, name, document, status FROM customers WHERE id = @id",
                conn);
            cmd.Parameters.AddWithValue("id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var customer = new
                {
                    id = reader.GetInt64(0),
                    name = reader.GetString(1),
                    document = reader.IsDBNull(2) ? null : reader.GetString(2),
                    status = reader.GetString(3)
                };

                return Ok(customer);
            }

            return NotFound(new { error = $"Customer {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

