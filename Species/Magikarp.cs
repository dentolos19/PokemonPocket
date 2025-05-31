// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Species;

public class Magikarp : Pokemon
{
    // Properties
    public override string Name => nameof(Magikarp);
    public override int MaxHealth => 80;
    public override int DamageMultiplier => 1;

    // Skill
    public override string SkillName => "Splash";
    public override int SkillDamage => 5;
}