// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Species;

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