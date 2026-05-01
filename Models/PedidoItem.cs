namespace JCLavanderia.Pedidos.Models;

/// <summary>
/// Entidade representando um item dentro de um pedido.
/// </summary>
public class PedidoItem
{
    /// <summary>Identificador único do item.</summary>
    public int Id { get; set; }

    /// <summary>Identificador do pedido ao qual este item pertence.</summary>
    public int PedidoId { get; set; }

    /// <summary>Pedido ao qual este item pertence.</summary>
    public Pedido Pedido { get; set; } = null!;

    /// <summary>Identificador do material/serviço.</summary>
    public int MaterialId { get; set; }

    /// <summary>Material/serviço associado a este item.</summary>
    public Material Material { get; set; } = null!;

    /// <summary>Quantidade solicitada do material.</summary>
    public int Quantidade { get; set; }

    /// <summary>Observação específica para este item.</summary>
    public string? Observacao { get; set; }
}
