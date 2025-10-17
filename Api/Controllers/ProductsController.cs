using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
    {
        var products = new[] {
            new { name = "Seguro Básico", price = 199.90 },
            new { name = "Proteção Total", price = 349.99 },
            new { name = "Garantia Estendida", price = 89.50 }
        };
        return Ok(products);
    }
}
