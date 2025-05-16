namespace PokemonPocket.Models;

public class PokemonMaster
{
    public string Name { get; set; }
    public int NoToEvolve { get; set; }
    public string EvolveTo { get; set; }

    public PokemonMaster(string name, int noToEvolve, string evolveTo)
    {
        Name = name;
        NoToEvolve = noToEvolve;
        EvolveTo = evolveTo;
    }

    public int GetEvolvableAmount(Pokemon[] pokemons)
    {
        var number = pokemons.Count(pokemon => pokemon.Name == Name);
        return number >= NoToEvolve ? (int)Math.Floor((double)(number / NoToEvolve)) : 0;
    }
}