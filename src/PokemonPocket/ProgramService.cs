// Catolos Alvaro Dennise Jay San Juan
// 231292A

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

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    #region Pets

    public void AddPet(Pokemon pokemon)
    {
        _context.Pets.Add(pokemon);
        SaveChanges();
    }

    public Pokemon[] GetAllPets()
    {
        return _context.Pets.ToArray();
    }

    public Pokemon[] GetPokemonPets(string pokemonName)
    {
        return _context.Pets.Where(pet => pet.Name.Equals(pokemonName)).ToArray();
    }

    public void RemovePet(Pokemon pokemon)
    {
        _context.Pets.Remove(pokemon);
        SaveChanges();
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

    public Pokemon[] GetAllPokemons()
    {
        return _pokemons.ToArray();
    }

    #endregion

    #region Masters

    public PokemonMaster[] GetAllMasters()
    {
        return _masters.ToArray();
    }

    public PokemonMaster? GetMaster(string name)
    {
        return _masters.FirstOrDefault(master => master.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    #endregion
}