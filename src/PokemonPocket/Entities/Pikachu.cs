using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Pikachu() : PokemonEntity("2B378027-CD66-4850-AD25-B8B022210F86")
{
    public override string Name => nameof(Pikachu);

    public override string SkillName => "Thunderbolt";
    public override int SkillDamage => 30;

    public override Type NextEvolutionType => typeof(Raichu);
    public override int MinimumEvolutionAmount => 2;
}