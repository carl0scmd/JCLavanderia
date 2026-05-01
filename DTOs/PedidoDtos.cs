using System.ComponentModel.DataAnnotations;
using JCLavanderia.Pedidos.Models;

namespace JCLavanderia.Pedidos.DTOs;

public class CriarPedidoItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Material é obrigatório")]
    public int MaterialId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
    public int Quantidade { get; set; }

    [StringLength(300, ErrorMessage = "Observação deve ter no máximo 300 caracteres")]
    public string? Observacao { get; set; }
}

public class CriarPedidoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Cliente é obrigatório")]
    public int ClienteId { get; set; }

    public DateTime? DataEntregaPrevista { get; set; }

    [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
    public string? Observacoes { get; set; }

    [MinLength(1, ErrorMessage = "Inclua ao menos um item no pedido")]
    public List<CriarPedidoItemRequest> Itens { get; set; } = [];
}

public class AtualizarStatusPedidoRequest
{
    [Required]
    public PedidoStatus? Status { get; set; }
}

public record PedidoItemResponse(
    int Id,
    int MaterialId,
    string MaterialNome,
    int Quantidade,
    string? Observacao)
{
    public static PedidoItemResponse FromEntity(PedidoItem item) =>
        new(item.Id, item.MaterialId, item.Material.Nome, item.Quantidade, item.Observacao);
}

public record PedidoResponse(
    int Id,
    int ClienteId,
    string ClienteNome,
    PedidoStatus Status,
    DateTime CriadoEm,
    DateTime AtualizadoEm,
    DateTime? DataEntregaPrevista,
    string? Observacoes,
    int QuantidadeTotalItens,
    List<PedidoItemResponse> Itens)
{
    public static PedidoResponse FromEntity(Pedido pedido)
    {
        var itens = pedido.Itens.Select(PedidoItemResponse.FromEntity).ToList();
        return new PedidoResponse(
            pedido.Id,
            pedido.ClienteId,
            pedido.Cliente.Nome,
            pedido.Status,
            pedido.CriadoEm,
            pedido.AtualizadoEm,
            pedido.DataEntregaPrevista,
            pedido.Observacoes,
            itens.Sum(i => i.Quantidade),
            itens);
    }
}
