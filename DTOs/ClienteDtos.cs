using System.ComponentModel.DataAnnotations;
using JCLavanderia.Pedidos.Models;

namespace JCLavanderia.Pedidos.DTOs;

public class CriarClienteRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 150 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string? Telefone { get; set; }

    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [StringLength(150, ErrorMessage = "E-mail deve ter no máximo 150 caracteres")]
    public string? Email { get; set; }

    [StringLength(300, ErrorMessage = "Endereço deve ter no máximo 300 caracteres")]
    public string? Endereco { get; set; }
}

public class AtualizarClienteRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 150 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string? Telefone { get; set; }

    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [StringLength(150, ErrorMessage = "E-mail deve ter no máximo 150 caracteres")]
    public string? Email { get; set; }

    [StringLength(300, ErrorMessage = "Endereço deve ter no máximo 300 caracteres")]
    public string? Endereco { get; set; }
}

public record ClienteResponse(
    int Id,
    string Nome,
    string? Telefone,
    string? Email,
    string? Endereco,
    DateTime CriadoEm)
{
    public static ClienteResponse FromEntity(Cliente cliente) =>
        new(
            cliente.Id,
            cliente.Nome,
            cliente.Telefone,
            cliente.Email,
            cliente.Endereco,
            cliente.CriadoEm);
}
