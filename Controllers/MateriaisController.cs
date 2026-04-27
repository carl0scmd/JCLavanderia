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
    public async Task<ActionResult<IEnumerable<MaterialResponse>>> Listar()
    {
        var materiais = await context.Materiais
            .AsNoTracking()
            .OrderBy(m => m.Nome)
            .ToListAsync();

        return Ok(materiais.Select(MaterialResponse.FromEntity));
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
        var material = new Material
        {
            Nome = request.Nome.Trim(),
            Descricao = string.IsNullOrWhiteSpace(request.Descricao) ? null : request.Descricao.Trim()
        };

        context.Materiais.Add(material);
        await context.SaveChangesAsync();

        var response = MaterialResponse.FromEntity(material);
        return CreatedAtAction(nameof(ObterPorId), new { id = material.Id }, response);
    }
}
