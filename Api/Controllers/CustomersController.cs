using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IConfiguration _cfg;

    public CustomersController(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery]string term)
    {
        var connString = _cfg.GetConnectionString("DefaultConnection");
        using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand("SELECT * FROM search_customers_by_name(@p)", conn);
        cmd.Parameters.AddWithValue("p", term);

        var result = new List<object>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new {
                id = reader.GetInt32(0),
                name = reader.GetString(1),
                document = reader.IsDBNull(2) ? null : reader.GetString(2),
                email = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }
        return Ok(result);
    }
}
