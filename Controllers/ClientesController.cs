using JCLavanderia.Pedidos.Data;
using JCLavanderia.Pedidos.DTOs;
using JCLavanderia.Pedidos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JCLavanderia.Pedidos.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ClienteResponse>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null)
    {
        (page, pageSize) = ControllerHelpers.NormalizePaging(page, pageSize);

        var query = context.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Nome.Contains(term) ||
                (c.Email != null && c.Email.Contains(term)) ||
                (c.Telefone != null && c.Telefone.Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var clientes = await query
            .OrderBy(c => c.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(PagedResult.Create(clientes.Select(ClienteResponse.FromEntity), totalCount, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteResponse>> ObterPorId(int id)
    {
        var cliente = await context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null)
        {
            return NotFound(new { message = "Cliente não encontrado." });
        }

        return Ok(ClienteResponse.FromEntity(cliente));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Criar([FromBody] CriarClienteRequest request)
    {
        if (ControllerHelpers.IsBlank(request.Nome))
        {
            return ControllerHelpers.BadRequestMessage("Nome é obrigatório.");
        }

        var cliente = new Cliente
        {
            Nome = ControllerHelpers.CleanRequired(request.Nome),
            Telefone = ControllerHelpers.CleanOptional(request.Telefone),
            Email = ControllerHelpers.CleanOptional(request.Email),
            Endereco = ControllerHelpers.CleanOptional(request.Endereco)
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, ClienteResponse.FromEntity(cliente));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteResponse>> Atualizar(int id, [FromBody] AtualizarClienteRequest request)
    {
        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null)
        {
            return NotFound(new { message = "Cliente não encontrado." });
        }

        if (ControllerHelpers.IsBlank(request.Nome))
        {
            return ControllerHelpers.BadRequestMessage("Nome é obrigatório.");
        }

        cliente.Nome = ControllerHelpers.CleanRequired(request.Nome);
        cliente.Telefone = ControllerHelpers.CleanOptional(request.Telefone);
        cliente.Email = ControllerHelpers.CleanOptional(request.Email);
        cliente.Endereco = ControllerHelpers.CleanOptional(request.Endereco);

        await context.SaveChangesAsync();

        return Ok(ClienteResponse.FromEntity(cliente));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await context.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound(new { message = "Cliente não encontrado." });
        }

        var pedidos = await context.Pedidos
            .Where(p => p.ClienteId == id)
            .ToListAsync();

        context.Pedidos.RemoveRange(pedidos);
        context.Clientes.Remove(cliente);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
