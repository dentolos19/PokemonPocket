// Dennise
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Species;

public class Geodude : Pokemon
{
    // Properties
    public override string Name => nameof(Geodude);
    public override int MaxHealth => 95;
    public override int DamageMultiplier => 1;

    // Skill
    public override string SkillName => "Rock Throw";
    public override int SkillDamage => 14;
}