using System.ComponentModel.DataAnnotations;
using JCLavanderia.Pedidos.Models;

namespace JCLavanderia.Pedidos.DTOs;

public class CriarClienteRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Telefone { get; set; }

    [EmailAddress]
    [StringLength(160)]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? Endereco { get; set; }
}

public class ClienteResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public DateTime CriadoEm { get; set; }

    public static ClienteResponse FromEntity(Cliente cliente) =>
        new()
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Telefone = cliente.Telefone,
            Email = cliente.Email,
            Endereco = cliente.Endereco,
            CriadoEm = cliente.CriadoEm
        };
}
