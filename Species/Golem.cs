// Dennise
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Species;

public class Golem : Pokemon
{
    // Properties
    public override string Name => nameof(Golem);
    public override int MaxHealth => 140;
    public override int DamageMultiplier => 5;

    // Skill
    public override string SkillName => "Earthquake";
    public override int SkillDamage => 55;
}