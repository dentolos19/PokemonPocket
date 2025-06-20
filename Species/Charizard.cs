// Dennise
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Species;

public class Charizard : Pokemon
{
    // Properties
    public override string Name => nameof(Charizard);
    public override int MaxHealth => 125;
    public override int DamageMultiplier => 5;

    // Skill
    public override string SkillName => "Fire Blast";
    public override int SkillDamage => 50;
}