// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Blastoise : Pokemon
{
    // Properties
    public override string Name => nameof(Blastoise);
    public override int MaxHealth => 130;
    public override int DamageMultiplier => 4;

    // Skill
    public override string SkillName => "Hydro Pump";
    public override int SkillDamage => 45;
}