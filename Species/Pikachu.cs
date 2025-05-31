// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Species;

public class Pikachu : Pokemon
{
    // Properties
    public override string Name => nameof(Pikachu);
    public override int MaxHealth => 100;
    public override int DamageMultiplier => 3;

    // Skill
    public override string SkillName => "Thunderbolt";
    public override int SkillDamage => 30;
}