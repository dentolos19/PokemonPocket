using PokemonPocket.Models;

namespace PokemonPocket.Entities;

public class Eevee() : PokemonEntity("578EE9BC-31D2-4325-B466-2C13ADC1D998")
{
    public override string Name => nameof(Eevee);

    public override string SkillName => "Run Away";
    public override int SkillDamage => 25;

    public override Type NextEvolutionType => typeof(Flareon);
    public override int MinimumEvolutionAmount => 3;
}