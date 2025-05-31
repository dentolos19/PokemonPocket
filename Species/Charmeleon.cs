// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Species;

public class Charmeleon : Pokemon
{
    // Properties
    public override string Name => nameof(Charmeleon);
    public override int MaxHealth => 100;
    public override int DamageMultiplier => 3;

    // Skill
    public override string SkillName => "Flame Burst";
    public override int SkillDamage => 30;
}