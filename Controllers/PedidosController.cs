using JCLavanderia.Pedidos.Data;
using JCLavanderia.Pedidos.DTOs;
using JCLavanderia.Pedidos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JCLavanderia.Pedidos.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PedidoResponse>>> Listar()
    {
        var pedidos = await context.Pedidos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .ThenInclude(i => i.Material)
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();

        return Ok(pedidos.Select(PedidoResponse.FromEntity));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoResponse>> ObterPorId(int id)
    {
        var pedido = await context.Pedidos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .ThenInclude(i => i.Material)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
        {
            return NotFound(new { message = "Pedido não encontrado." });
        }

        return Ok(PedidoResponse.FromEntity(pedido));
    }

    [HttpPost]
    public async Task<ActionResult<PedidoResponse>> Criar([FromBody] CriarPedidoRequest request)
    {
        var clienteExiste = await context.Clientes.AnyAsync(c => c.Id == request.ClienteId);
        if (!clienteExiste)
        {
            return BadRequest(new { message = "Cliente informado não existe." });
        }

        var materialIds = request.Itens.Select(i => i.MaterialId).Distinct().ToList();
        var materiaisExistentes = await context.Materiais
            .Where(m => materialIds.Contains(m.Id))
            .Select(m => m.Id)
            .ToListAsync();

        var faltantes = materialIds.Except(materiaisExistentes).ToList();
        if (faltantes.Count > 0)
        {
            return BadRequest(new { message = "Um ou mais materiais informados não existem.", materiaisInvalidos = faltantes });
        }

        var pedido = new Pedido
        {
            ClienteId = request.ClienteId,
            DataEntregaPrevista = request.DataEntregaPrevista,
            Observacoes = string.IsNullOrWhiteSpace(request.Observacoes) ? null : request.Observacoes.Trim(),
            Status = PedidoStatus.Recebido,
            Itens = request.Itens.Select(i => new PedidoItem
            {
                MaterialId = i.MaterialId,
                Quantidade = i.Quantidade,
                Observacao = string.IsNullOrWhiteSpace(i.Observacao) ? null : i.Observacao.Trim()
            }).ToList()
        };

        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync();

        var pedidoCriado = await context.Pedidos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .ThenInclude(i => i.Material)
            .FirstAsync(p => p.Id == pedido.Id);

        return CreatedAtAction(nameof(ObterPorId), new { id = pedidoCriado.Id }, PedidoResponse.FromEntity(pedidoCriado));
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<PedidoResponse>> AtualizarStatus(int id, [FromBody] AtualizarStatusPedidoRequest request)
    {
        if (request.Status is null)
        {
            return BadRequest(new { message = "Status é obrigatório." });
        }

        var pedido = await context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .ThenInclude(i => i.Material)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
        {
            return NotFound(new { message = "Pedido não encontrado." });
        }

        pedido.Status = request.Status.Value;
        await context.SaveChangesAsync();

        return Ok(PedidoResponse.FromEntity(pedido));
    }
}
