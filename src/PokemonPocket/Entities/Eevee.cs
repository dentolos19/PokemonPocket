using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Eevee : Pokemon
{
    // Properties
    public override string Name => nameof(Eevee);
    public override int MaxHealth => 100;
    public override int DamageMultiplier => 2;

    // Skill
    public override string SkillName => "Run Away";
    public override int SkillDamage => 25;
}