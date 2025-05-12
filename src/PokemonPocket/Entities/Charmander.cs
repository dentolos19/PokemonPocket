using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Charmander() : PokemonEntity("813E039C-7EFA-4AB5-A2F9-A04FBF40EF2A")
{
    public override string Name => nameof(Charmander);

    public override string SkillName => "Solar";
    public override int SkillDamage => 10;

    public override Type NextEvolutionType => typeof(Charmeleon);
    public override int MinimumEvolutionAmount => 1;
}