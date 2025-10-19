using Api.Models;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(ICustomerService customerService, ILogger<CustomersController> logger) : ControllerBase
{
    private readonly ICustomerService _customerService = customerService;
    private readonly ILogger<CustomersController> _logger = logger;

    /// <summary>
    /// Busca um cliente pelo nome (parcial ou completo)
    /// </summary>
    /// <param name="term">Termo de busca para o cliente</param>
    /// <param name="limit">Limite de resultados a serem retornados</param>
    /// <param name="offset">Deslocamento para paginação</param>
    /// <returns>Resultado da busca de clientes</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string term,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        var request = new CustomerSearchRequest
        {
            Term = term ?? string.Empty,
            Limit = limit,
            Offset = offset
        };

        var result = await _customerService.SearchByNameAsync(request);

        if (!result.Success)
        {
            return result.Message?.Contains("not found") == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Retorna um cliente pelo ID
    /// </summary>
    /// <param name="id">Id do cliente a ser buscado</param>
    /// <returns>Resultado da busca do cliente</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _customerService.GetByIdAsync(id);

        if (!result.Success)
        {
            return result.Message?.Contains("not found") == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cria um novo cliente
    /// </summary>
    /// <param name="customerDto">Dados do cliente a ser criado</param>
    /// <returns>Resultado da criação do cliente</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerDto customerDto)
    {
        var result = await _customerService.CreateAsync(customerDto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Atualiza um cliente existente
    /// </summary>
    /// <param name="id">Id do cliente a ser atualizado</param>
    /// <param name="customerDto">Dados do cliente a serem atualizados</param>
    /// <returns>Resultado da atualização do cliente</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] CustomerDto customerDto)
    {
        var result = await _customerService.UpdateAsync(id, customerDto);

        if (!result.Success)
        {
            return result.Message?.Contains("not found") == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Apaga um cliente pelo ID
    /// </summary>
    /// <param name="id">Id do cliente a ser apagado</param>
    /// <returns>Resultado da deleção do cliente</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _customerService.DeleteAsync(id);

        if (!result.Success)
        {
            return result.Message?.Contains("not found") == true
                ? NotFound(result)
                : BadRequest(result);
        }

        return Ok(result);
    }
}

