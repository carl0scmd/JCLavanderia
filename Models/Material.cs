namespace JCLavanderia.Pedidos.Models;

/// <summary>
/// Entidade representando um material/serviço disponível para pedidos.
/// </summary>
public class Material
{
    /// <summary>Identificador único do material.</summary>
    public int Id { get; set; }

    /// <summary>Nome do material ou serviço.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Descrição detalhada do material.</summary>
    public string? Descricao { get; set; }

    /// <summary>Data e hora em que o material foi cadastrado.</summary>
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    /// <summary>Itens de pedido que usam este material.</summary>
    public ICollection<PedidoItem> PedidoItens { get; set; } = new List<PedidoItem>();
}
