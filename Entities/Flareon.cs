using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Flareon() : PokemonEntity("F7BBC4DB-4941-4A4D-8235-04B501D27A45")
{
    public override string Name => nameof(Flareon);

    public override string SkillName => "Fire Spin";
    public override int SkillDamage => 40;
}