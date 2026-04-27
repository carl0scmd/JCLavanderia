namespace JCLavanderia.Pedidos.Models;

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public PedidoStatus Status { get; set; } = PedidoStatus.Recebido;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? DataEntregaPrevista { get; set; }
    public string? Observacoes { get; set; }
    public ICollection<PedidoItem> Itens { get; set; } = new List<PedidoItem>();
}
