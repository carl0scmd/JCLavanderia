namespace JCLavanderia.Pedidos.Models;

public class Material
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public ICollection<PedidoItem> PedidoItens { get; set; } = new List<PedidoItem>();
}
