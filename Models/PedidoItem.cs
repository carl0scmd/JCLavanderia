namespace JCLavanderia.Pedidos.Models;

public class PedidoItem
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public int MaterialId { get; set; }
    public Material Material { get; set; } = null!;
    public int Quantidade { get; set; }
    public string? Observacao { get; set; }
}
