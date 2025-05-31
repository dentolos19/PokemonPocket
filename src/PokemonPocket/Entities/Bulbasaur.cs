// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Bulbasaur : Pokemon
{
    // Properties
    public override string Name => nameof(Bulbasaur);
    public override int MaxHealth => 105;
    public override int DamageMultiplier => 1;

    // Skill
    public override string SkillName => "Vine Whip";
    public override int SkillDamage => 12;
}