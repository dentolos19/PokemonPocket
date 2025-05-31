// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Vaporeon : Pokemon
{
    // Properties
    public override string Name => nameof(Vaporeon);
    public override int MaxHealth => 120;
    public override int DamageMultiplier => 4;

    // Skill
    public override string SkillName => "Aqua Ring";
    public override int SkillDamage => 35;
}