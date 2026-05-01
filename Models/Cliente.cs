namespace JCLavanderia.Pedidos.Models;

/// <summary>
/// Entidade representando um cliente da lavanderia.
/// </summary>
public class Cliente
{
    /// <summary>Identificador único do cliente.</summary>
    public int Id { get; set; }

    /// <summary>Nome completo do cliente.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Número de telefone para contato.</summary>
    public string? Telefone { get; set; }

    /// <summary>Endereço de e-mail do cliente.</summary>
    public string? Email { get; set; }

    /// <summary>Endereço de entrega do cliente.</summary>
    public string? Endereco { get; set; }

    /// <summary>Data e hora em que o cliente foi cadastrado.</summary>
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    /// <summary>Pedidos feitos pelo cliente.</summary>
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
