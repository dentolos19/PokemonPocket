// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Gyarados : Pokemon
{
    // Properties
    public override string Name => nameof(Gyarados);
    public override int MaxHealth => 150;
    public override int DamageMultiplier => 6;

    // Skill
    public override string SkillName => "Hyper Beam";
    public override int SkillDamage => 60;
}