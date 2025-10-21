using Api.DTOs;
using Api.Extensions;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService, ILogger<ProductsController> logger) : ControllerBase
{
    private readonly IProductService _productService = productService;
    private readonly ILogger<ProductsController> _logger = logger;

    /// <summary>
    /// Retorna todos os produtos (ativos e inativos)
    /// </summary>
    /// <returns>Lista de Produtos</returns>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var result = await _productService.GetAllAsync();
            return result.ToActionResult(); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produtos");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Retorna apenas produtos ativos
    /// </summary>
    /// <param name="id">Id do produto para obter</param>
    /// <returns>Produto</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _productService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Cria um novo produto
    /// </summary>
    /// <param name="productDto">Dados do produto a ser criado</param>
    /// <returns>Resultado da criação do produto</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductDto productDto)
    {
        var result = await _productService.CreateAsync(productDto);
        return result.ToActionResult();
    }

    /// <summary>
    /// Atualiza um produto existente
    /// </summary>
    /// <param name="id">Id do produto a ser atualizado</param>
    /// <param name="productDto">Dados do produto a serem atualizados</param>
    /// <returns>Resultado da atualização do produto</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] ProductDto productDto)
    {
        var result = await _productService.UpdateAsync(id, productDto);
        return result.ToActionResult();
    }

    /// <summary>
    /// Remove um produto por ID
    /// </summary>
    /// <param name="id">Id do produto a ser removido</param>
    /// <returns>Resultado da remoção do produto</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _productService.DeleteAsync(id);
        return result.ToActionResult();
    }
}

