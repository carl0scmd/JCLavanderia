namespace JCLavanderia.Pedidos.Models;

public enum PedidoStatus
{
    Recebido = 1,
    EmAndamento = 2,
    Pronto = 3,
    Entregue = 4,
    Cancelado = 5,

    // Compatibilidade com versões anteriores do frontend/script.
    EmProcessamento = EmAndamento,
    ProntoParaEntrega = Pronto
}
