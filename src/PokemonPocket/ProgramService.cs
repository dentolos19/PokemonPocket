using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PokemonPocket.Entities;
using PokemonPocket.Models;

namespace PokemonPocket;

public class ProgramService
{
    private readonly ProgramDatabase _context;
    private readonly List<Pokemon> _pokemons;
    private readonly List<PokemonMaster> _masters;

    public ProgramService()
    {
        _context = new ProgramDatabase();

        // Initialize Pokemon Masters
        _masters = new List<PokemonMaster>
        {
            new(nameof(Pikachu), 2, nameof(Raichu)),
            new(nameof(Eevee), 3, nameof(Flareon)),
            new(nameof(Charmander), 1, nameof(Charmeleon))
        };

        // Load Available Pokemons
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Pokemon)));
        var pokemons = types.Select(type => (Pokemon)Activator.CreateInstance(type)!);
        _pokemons = pokemons.ToList();

        // Ensure the database is created and migrated
        _context.Database.EnsureCreated();
        _context.Database.Migrate();
    }

    #region Pets

    public void AddPet(Pokemon pokemon)
    {
        _context.OwnedPokemons.Add(pokemon);
    }

    public Pokemon[] GetAllPets()
    {
        return _context.OwnedPokemons.ToArray();
    }

    public void RemovePet(Pokemon pokemon)
    {
        _context.OwnedPokemons.Remove(pokemon);
    }

    #endregion

    #region Pokemons

    public bool CheckPokemonExists(string name)
    {
        return _pokemons.Any(pokemon => pokemon.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public Pokemon? GetPokemon(string name)
    {
        return _pokemons.FirstOrDefault(pokemon =>
            pokemon.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Masters

    public PokemonMaster[] GetAllMasters()
    {
        return _masters.ToArray();
    }

    #endregion
}