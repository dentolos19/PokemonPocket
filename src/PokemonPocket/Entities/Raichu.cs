using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Raichu() : PokemonEntity("945EC3E2-7831-4F26-89D6-CD57C2191316")
{
    public override string Name => nameof(Raichu);

    public override string SkillName => "Enhanced Thunderbolt";
    public override int SkillDamage => 50;
}