// Dennise
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Species;

public class Venusaur : Pokemon
{
    // Properties
    public override string Name => nameof(Venusaur);
    public override int MaxHealth => 135;
    public override int DamageMultiplier => 4;

    // Skill
    public override string SkillName => "Solar Beam";
    public override int SkillDamage => 48;
}