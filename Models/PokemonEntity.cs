using System.ComponentModel.DataAnnotations;

namespace PokemonPocket.Models;

public abstract class PokemonEntity(string id)
{
    // Description
    [Key] public Guid Id { get; set; } = new(id);
    public virtual string Name { get; }

    // Skill
    public virtual string SkillName => "Basic Attack";
    public virtual int SkillDamage => 10;

    // Evolution
    public virtual Type? NextEvolutionType => null;
    public virtual int MinimumEvolutionAmount => 0;
    public virtual int MinimumEvolutionExperience => 0;
}