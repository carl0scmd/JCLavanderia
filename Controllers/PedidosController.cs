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
    private static readonly IReadOnlyDictionary<PedidoStatus, PedidoStatus[]> AllowedTransitions =
        new Dictionary<PedidoStatus, PedidoStatus[]>
        {
            [PedidoStatus.Recebido] = [PedidoStatus.EmAndamento, PedidoStatus.Cancelado],
            [PedidoStatus.EmAndamento] = [PedidoStatus.Pronto, PedidoStatus.Cancelado],
            [PedidoStatus.Pronto] = [PedidoStatus.Entregue, PedidoStatus.Cancelado],
            [PedidoStatus.Entregue] = [],
            [PedidoStatus.Cancelado] = []
        };

    [HttpGet]
    public async Task<ActionResult<PagedResult<PedidoResponse>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] PedidoStatus? status = null,
        [FromQuery] int? clienteId = null)
    {
        (page, pageSize) = ControllerHelpers.NormalizePaging(page, pageSize);

        var query = context.Pedidos.AsNoTracking();
        if (status is not null)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (clienteId is not null)
        {
            query = query.Where(p => p.ClienteId == clienteId.Value);
        }

        var totalCount = await query.CountAsync();
        var pedidos = await query
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .ThenInclude(i => i.Material)
            .OrderByDescending(p => p.CriadoEm)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(PagedResult.Create(pedidos.Select(PedidoResponse.FromEntity), totalCount, page, pageSize));
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
        if (request.DataEntregaPrevista is not null && request.DataEntregaPrevista.Value.Date < DateTime.UtcNow.Date)
        {
            return ControllerHelpers.BadRequestMessage("Data de entrega prevista não pode estar no passado.");
        }

        if (request.Itens.Count == 0)
        {
            return ControllerHelpers.BadRequestMessage("Inclua ao menos um item no pedido.");
        }

        var clienteExiste = await context.Clientes.AnyAsync(c => c.Id == request.ClienteId);
        if (!clienteExiste)
        {
            return ControllerHelpers.BadRequestMessage("Cliente informado não existe.");
        }

        var materialIds = request.Itens.Select(i => i.MaterialId).Distinct().ToArray();
        var materiaisExistentes = await context.Materiais
            .Where(m => materialIds.Contains(m.Id))
            .Select(m => m.Id)
            .ToListAsync();

        var faltantes = materialIds.Except(materiaisExistentes).ToArray();
        if (faltantes.Length > 0)
        {
            return BadRequest(new { message = "Um ou mais materiais informados não existem.", materiaisInvalidos = faltantes });
        }

        var itens = request.Itens
            .GroupBy(i => new { i.MaterialId, Observacao = ControllerHelpers.CleanOptional(i.Observacao) })
            .Select(g => new PedidoItem
            {
                MaterialId = g.Key.MaterialId,
                Quantidade = g.Sum(i => i.Quantidade),
                Observacao = g.Key.Observacao
            })
            .ToList();

        var pedido = new Pedido
        {
            ClienteId = request.ClienteId,
            DataEntregaPrevista = request.DataEntregaPrevista,
            Observacoes = ControllerHelpers.CleanOptional(request.Observacoes),
            Status = PedidoStatus.Recebido,
            Itens = itens
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
            return ControllerHelpers.BadRequestMessage("Status é obrigatório.");
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

        if (pedido.Status == request.Status.Value)
        {
            return Ok(PedidoResponse.FromEntity(pedido));
        }

        if (!AllowedTransitions[pedido.Status].Contains(request.Status.Value))
        {
            return BadRequest(new
            {
                message = $"Transição de status inválida: {pedido.Status} para {request.Status.Value}."
            });
        }

        pedido.Status = request.Status.Value;
        await context.SaveChangesAsync();

        return Ok(PedidoResponse.FromEntity(pedido));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pedido = await context.Pedidos.FindAsync(id);
        if (pedido is null)
        {
            return NotFound(new { message = "Pedido não encontrado." });
        }

        context.Pedidos.Remove(pedido);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
