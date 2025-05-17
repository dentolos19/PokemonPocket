using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Charmander : Pokemon
{
    // Properties
    public override string Name => nameof(Charmander);
    public override int MaxHealth => 100;

    // Properties
    public override int DamageMultiplier => 1;

    // Skill
    public override string SkillName => "Solar Power";
    public override int SkillDamage => 10;
}