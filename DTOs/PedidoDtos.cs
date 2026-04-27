using System.ComponentModel.DataAnnotations;
using JCLavanderia.Pedidos.Models;

namespace JCLavanderia.Pedidos.DTOs;

public class CriarPedidoItemRequest
{
    [Range(1, int.MaxValue)]
    public int MaterialId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; }

    [StringLength(300)]
    public string? Observacao { get; set; }
}

public class CriarPedidoRequest
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }

    public DateTime? DataEntregaPrevista { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    [MinLength(1)]
    public List<CriarPedidoItemRequest> Itens { get; set; } = [];
}

public class AtualizarStatusPedidoRequest
{
    [Required]
    public PedidoStatus? Status { get; set; }
}

public class PedidoItemResponse
{
    public int Id { get; set; }
    public int MaterialId { get; set; }
    public string MaterialNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public string? Observacao { get; set; }

    public static PedidoItemResponse FromEntity(PedidoItem item) =>
        new()
        {
            Id = item.Id,
            MaterialId = item.MaterialId,
            MaterialNome = item.Material.Nome,
            Quantidade = item.Quantidade,
            Observacao = item.Observacao
        };
}

public class PedidoResponse
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public PedidoStatus Status { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? DataEntregaPrevista { get; set; }
    public string? Observacoes { get; set; }
    public int QuantidadeTotalItens { get; set; }
    public List<PedidoItemResponse> Itens { get; set; } = [];

    public static PedidoResponse FromEntity(Pedido pedido)
    {
        var itens = pedido.Itens.Select(PedidoItemResponse.FromEntity).ToList();
        return new PedidoResponse
        {
            Id = pedido.Id,
            ClienteId = pedido.ClienteId,
            ClienteNome = pedido.Cliente.Nome,
            Status = pedido.Status,
            CriadoEm = pedido.CriadoEm,
            DataEntregaPrevista = pedido.DataEntregaPrevista,
            Observacoes = pedido.Observacoes,
            QuantidadeTotalItens = itens.Sum(i => i.Quantidade),
            Itens = itens
        };
    }
}
