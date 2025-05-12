using System.ComponentModel.DataAnnotations;

namespace PokemonPocket.Models;

public class PokemonPet
{
    [Key] public Guid Id { get; init; } = Guid.NewGuid();
    public Guid EntityId { get; init; }

    public string? Name { get; set; }
    public int Health { get; set; }
    public int Experience { get; set; }

    public PokemonEntity? GetEntity()
    {
        return Program.Service.GetEntity(EntityId);
    }

    public string GetName()
    {
        return string.IsNullOrEmpty(Name) ? GetEntity()?.Name ?? "Unknown" : Name;
    }
}