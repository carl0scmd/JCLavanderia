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

        var pedidoIdsComMaterial = await context.PedidoItens
            .Where(i => i.MaterialId == id)
            .Select(i => i.PedidoId)
            .Distinct()
            .ToListAsync();

        var pedidoIdsParaExcluir = pedidoIdsComMaterial.Count == 0
            ? []
            : await context.PedidoItens
                .Where(i => pedidoIdsComMaterial.Contains(i.PedidoId))
                .GroupBy(i => i.PedidoId)
                .Where(g => g.All(i => i.MaterialId == id))
                .Select(g => g.Key)
                .ToListAsync();

        var itensParaRemover = await context.PedidoItens
            .Where(i => i.MaterialId == id)
            .ToListAsync();

        var pedidosParaExcluir = await context.Pedidos
            .Where(p => pedidoIdsParaExcluir.Contains(p.Id))
            .ToListAsync();

        await using var transaction = await context.Database.BeginTransactionAsync();

        context.PedidoItens.RemoveRange(itensParaRemover);
        await context.SaveChangesAsync();

        context.Pedidos.RemoveRange(pedidosParaExcluir);
        await context.SaveChangesAsync();

        context.Materiais.Remove(material);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return NoContent();
    }
}
