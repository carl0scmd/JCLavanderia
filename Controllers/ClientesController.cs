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
    public async Task<ActionResult<IEnumerable<ClienteResponse>>> Listar()
    {
        var clientes = await context.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync();

        return Ok(clientes.Select(ClienteResponse.FromEntity));
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
        var cliente = new Cliente
        {
            Nome = request.Nome.Trim(),
            Telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Endereco = string.IsNullOrWhiteSpace(request.Endereco) ? null : request.Endereco.Trim()
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var response = ClienteResponse.FromEntity(cliente);
        return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, response);
    }
}
