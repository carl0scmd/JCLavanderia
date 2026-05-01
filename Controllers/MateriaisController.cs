using JCLavanderia.Pedidos.Data;
using JCLavanderia.Pedidos.DTOs;
using JCLavanderia.Pedidos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JCLavanderia.Pedidos.Controllers;

[ApiController]
[Route("api/materiais")]
public class MateriaisController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<MaterialResponse>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null)
    {
        (page, pageSize) = ControllerHelpers.NormalizePaging(page, pageSize);

        var query = context.Materiais.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m =>
                m.Nome.Contains(term) ||
                (m.Descricao != null && m.Descricao.Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var materiais = await query
            .OrderBy(m => m.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(PagedResult.Create(materiais.Select(MaterialResponse.FromEntity), totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MaterialResponse>> ObterPorId(int id)
    {
        var material = await context.Materiais.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (material is null)
        {
            return NotFound(new { message = "Material não encontrado." });
        }

        return Ok(MaterialResponse.FromEntity(material));
    }

    [HttpPost]
    public async Task<ActionResult<MaterialResponse>> Criar([FromBody] CriarMaterialRequest request)
    {
        if (ControllerHelpers.IsBlank(request.Nome))
        {
            return ControllerHelpers.BadRequestMessage("Nome é obrigatório.");
        }

        var material = new Material
        {
            Nome = ControllerHelpers.CleanRequired(request.Nome),
            Descricao = ControllerHelpers.CleanOptional(request.Descricao)
        };

        context.Materiais.Add(material);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = material.Id }, MaterialResponse.FromEntity(material));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var material = await context.Materiais.FindAsync(id);
        if (material is null)
        {
            return NotFound(new { message = "Material não encontrado." });
        }

        var emUso = await context.PedidoItens.AnyAsync(i => i.MaterialId == id);
        if (emUso)
        {
            return BadRequest(new { message = "Material está em uso em um pedido e não pode ser excluído." });
        }

        context.Materiais.Remove(material);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
