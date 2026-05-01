namespace JCLavanderia.Pedidos.Models;

/// <summary>
/// Entidade representando um pedido de lavanderia.
/// </summary>
public class Pedido
{
    /// <summary>Identificador único do pedido.</summary>
    public int Id { get; set; }

    /// <summary>Identificador do cliente que fez o pedido.</summary>
    public int ClienteId { get; set; }

    /// <summary>Cliente associado ao pedido.</summary>
    public Cliente Cliente { get; set; } = null!;

    /// <summary>Status atual do pedido.</summary>
    public PedidoStatus Status { get; set; } = PedidoStatus.Recebido;

    /// <summary>Data e hora em que o pedido foi criado.</summary>
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    /// <summary>Data e hora da última atualização do pedido.</summary>
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    /// <summary>Data prevista para entrega do pedido.</summary>
    public DateTime? DataEntregaPrevista { get; set; }

    /// <summary>Observações gerais do pedido.</summary>
    public string? Observacoes { get; set; }

    /// <summary>Itens incluídos no pedido.</summary>
    public ICollection<PedidoItem> Itens { get; set; } = new List<PedidoItem>();
}
