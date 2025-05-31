// Dennise
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Species;

public class Wartortle : Pokemon
{
    // Properties
    public override string Name => nameof(Wartortle);
    public override int MaxHealth => 110;
    public override int DamageMultiplier => 2;

    // Skill
    public override string SkillName => "Bubble Beam";
    public override int SkillDamage => 25;
}