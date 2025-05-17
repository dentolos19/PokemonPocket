using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Flareon : Pokemon
{
    // Properties
    public override string Name => nameof(Flareon);
    public override int MaxHealth => 100;
    public override int DamageMultiplier => 3;

    // Skill
    public override string SkillName => "Thunderbolt";
    public override int SkillDamage => 30;
}