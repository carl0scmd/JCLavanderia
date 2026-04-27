using System.ComponentModel.DataAnnotations;
using JCLavanderia.Pedidos.Models;

namespace JCLavanderia.Pedidos.DTOs;

public class CriarMaterialRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descricao { get; set; }
}

public class MaterialResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime CriadoEm { get; set; }

    public static MaterialResponse FromEntity(Material material) =>
        new()
        {
            Id = material.Id,
            Nome = material.Nome,
            Descricao = material.Descricao,
            CriadoEm = material.CriadoEm
        };
}
