using System.ComponentModel.DataAnnotations;
using JCLavanderia.Pedidos.Models;

namespace JCLavanderia.Pedidos.DTOs;

public class CriarMaterialRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 150 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Descrição deve ter no máximo 300 caracteres")]
    public string? Descricao { get; set; }
}

public record MaterialResponse(int Id, string Nome, string? Descricao, DateTime CriadoEm)
{
    public static MaterialResponse FromEntity(Material material) =>
        new(material.Id, material.Nome, material.Descricao, material.CriadoEm);
}
