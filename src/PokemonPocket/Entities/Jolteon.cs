// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Jolteon : Pokemon
{
    // Properties
    public override string Name => nameof(Jolteon);
    public override int MaxHealth => 110;
    public override int DamageMultiplier => 4;

    // Skill
    public override string SkillName => "Thunder";
    public override int SkillDamage => 40;
}