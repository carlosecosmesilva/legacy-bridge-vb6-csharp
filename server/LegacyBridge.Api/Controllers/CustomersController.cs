using Microsoft.AspNetCore.Mvc;
using LegacyBridge.Api.Extensions;
using LegacyBridge.Application.DTOs;
using LegacyBridge.Application.Contracts.Requests;
using LegacyBridge.Application.Interfaces;

namespace LegacyBridge.Api.Controllers;

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
    /// <param name="offset">Deslocamento para pagina��o</param>
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

        return result.ToActionResult();
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
        return result.ToActionResult();
    }

    /// <summary>
    /// Cria um novo cliente
    /// </summary>
    /// <param name="customerDto">Dados do cliente a ser criado</param>
    /// <returns>Resultado da cria��o do cliente</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerDto customerDto)
    {
        var result = await _customerService.CreateAsync(customerDto);
        return result.ToActionResult();
    }

    /// <summary>
    /// Atualiza um cliente existente
    /// </summary>
    /// <param name="id">Id do cliente a ser atualizado</param>
    /// <param name="customerDto">Dados do cliente a serem atualizados</param>
    /// <returns>Resultado da atualiza��o do cliente</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] CustomerDto customerDto)
    {
        var result = await _customerService.UpdateAsync(id, customerDto);
        return result.ToActionResult();
    }

    /// <summary>
    /// Apaga um cliente pelo ID
    /// </summary>
    /// <param name="id">Id do cliente a ser apagado</param>
    /// <returns>Resultado da dele��o do cliente</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _customerService.DeleteAsync(id);
        return result.ToActionResult();
    }
}

