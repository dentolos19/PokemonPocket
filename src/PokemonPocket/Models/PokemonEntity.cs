using System.ComponentModel.DataAnnotations;

namespace PokemonPocket.Models;

public abstract class PokemonEntity(string id)
{
    // Description
    [Key] public Guid Id { get; set; } = new(id);
    public virtual string Name { get; }
    public virtual int MaxHealth => 100;

    // Skill
    public virtual string SkillName => "Basic Attack";
    public virtual int SkillDamage => 10;

    // Evolution
    public virtual Type? NextEvolutionType => null;
    public virtual int MinimumEvolutionAmount => 0;
    public virtual int MinimumEvolutionExperience => 0;

    public virtual int DealDamage()
    {
        return Random.Shared.Next(SkillDamage - 5, SkillDamage + 5);
    }

    public PokemonPet SpawnPet()
    {
        return new PokemonPet
        {
            EntityId = Id,
            Health = MaxHealth,
            Experience = 0
        };
    }
}