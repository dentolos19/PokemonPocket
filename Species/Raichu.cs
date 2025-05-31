// Dennise
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Species;

public class Raichu : Pokemon
{
    // Properties
    public override string Name => nameof(Raichu);
    public override int MaxHealth => 100;
    public override int DamageMultiplier => 3;

    // Skill
    public override string SkillName => "Thunderbolt";
    public override int SkillDamage => 30;
}