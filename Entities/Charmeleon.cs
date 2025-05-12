using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Charmeleon() : PokemonEntity("27170FFD-CA65-4B7B-A814-28C5F4509F94")
{
    public override string Name => nameof(Charmeleon);

    public override string SkillName => "Flame Burst";
    public override int SkillDamage => 30;
}