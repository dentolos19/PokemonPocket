// Dennise
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Species;

public class Squirtle : Pokemon
{
    // Properties
    public override string Name => nameof(Squirtle);
    public override int MaxHealth => 100;
    public override int DamageMultiplier => 1;

    // Skill
    public override string SkillName => "Water Gun";
    public override int SkillDamage => 15;
}