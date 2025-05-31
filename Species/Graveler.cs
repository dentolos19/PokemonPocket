// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Species;

public class Graveler : Pokemon
{
    // Properties
    public override string Name => nameof(Graveler);
    public override int MaxHealth => 115;
    public override int DamageMultiplier => 3;

    // Skill
    public override string SkillName => "Rock Slide";
    public override int SkillDamage => 32;
}