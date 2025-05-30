// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Charmeleon : Pokemon
{
    // Properties
    public override string Name => nameof(Charmeleon);
    public override int MaxHealth => 100;
    public override int DamageMultiplier => 3;

    // Skill
    public override string SkillName => "Thunderbolt";
    public override int SkillDamage => 30;
}