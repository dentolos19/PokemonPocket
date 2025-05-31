// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Ivysaur : Pokemon
{
    // Properties
    public override string Name => nameof(Ivysaur);
    public override int MaxHealth => 115;
    public override int DamageMultiplier => 2;

    // Skill
    public override string SkillName => "Razor Leaf";
    public override int SkillDamage => 28;
}