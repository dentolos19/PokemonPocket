using PokemonPocket.Models;

namespace PokemonPocket.Helpers;

public static class Utilities
{
    public static string FindChainStart(string speciesName, PokemonMaster[] masters)
    {
        // Look for any master that evolves into this species
        var previousMaster = masters.FirstOrDefault(master => master.EvolveTo == speciesName);
        if (previousMaster == null)
            return speciesName;

        // Recursively find the start of the chain
        return FindChainStart(previousMaster.Name, masters);
    }
}