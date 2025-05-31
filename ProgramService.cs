// Dennise
// 231292A

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PokemonPocket.Models;
using PokemonPocket.Species;

namespace PokemonPocket;

public class ProgramService
{
    private readonly ProgramDatabase _context;
    private readonly List<Pokemon> _species;
    private readonly List<PokemonMaster> _masters;

    public ProgramService()
    {
        // Setup Database
        _context = new ProgramDatabase();

        // Load Masters
        _masters = new List<PokemonMaster>
        {
            new(nameof(Pikachu), 2, nameof(Raichu)),
            new(nameof(Eevee), 3, nameof(Flareon)),
            new(nameof(Charmander), 1, nameof(Charmeleon)),
            new(nameof(Eevee), 3, nameof(Vaporeon)),
            new(nameof(Eevee), 3, nameof(Jolteon)),
            new(nameof(Charmeleon), 2, nameof(Charizard)),
            new(nameof(Squirtle), 1, nameof(Wartortle)),
            new(nameof(Wartortle), 2, nameof(Blastoise)),
            new(nameof(Bulbasaur), 1, nameof(Ivysaur)),
            new(nameof(Ivysaur), 2, nameof(Venusaur)),
            new(nameof(Geodude), 2, nameof(Graveler)),
            new(nameof(Graveler), 3, nameof(Golem)),
            new(nameof(Magikarp), 5, nameof(Gyarados))
        };

        // Load Available Species
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Pokemon)));
        var species = types.Select(type => (Pokemon)Activator.CreateInstance(type)!);
        _species = species.ToList();

        // Ensure the database is created and migrated
        _context.Database.EnsureCreated();
        _context.Database.Migrate();
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }

    public void AddPokemon(Pokemon pokemon)
    {
        _context.Pokemons.Add(pokemon);
        SaveChanges();
    }

    public Pokemon[] GetAllPokemons()
    {
        return _context.Pokemons.OrderBy(pet => pet.Name).ToArray();
    }

    public Pokemon[] GetPokemonsBySpecies(string speciesName)
    {
        return _context.Pokemons.Where(pet => pet.Name.Equals(speciesName)).ToArray();
    }

    public void RemovePokemon(Pokemon pokemon)
    {
        // Ensure the entity is being tracked by the context
        var trackedPet = _context.Pokemons.Local.FirstOrDefault(localPokemon => localPokemon.Id == pokemon.Id);

        if (trackedPet != null)
        {
            _context.Pokemons.Remove(trackedPet);
        }
        else
        {
            // If not tracked, attach it first, then remove
            var removablePet = _context.Pokemons.Find(pokemon.Id);
            if (removablePet != null)
            {
                _context.Pokemons.Remove(removablePet);
            }
        }

        SaveChanges();
    }

    public void RemovePokemons(IEnumerable<Pokemon> pokemons)
    {
        using var transaction = _context.Database.BeginTransaction();

        try
        {
            foreach (var pokemon in pokemons.ToList())
            {
                var trackedPet = _context.Pokemons.Local.FirstOrDefault(localPokemon => localPokemon.Id == pokemon.Id);

                if (trackedPet != null)
                {
                    _context.Pokemons.Remove(trackedPet);
                }
                else
                {
                    // If not tracked, attach it first, then remove
                    var removableEntity = _context.Pokemons.Find(pokemon.Id);
                    if (removableEntity != null)
                    {
                        _context.Pokemons.Remove(removableEntity);
                    }
                }
            }

            SaveChanges();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public bool CheckSpeciesExists(string name)
    {
        return _species.Any(pokemon => pokemon.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public Pokemon? GetSpecies(string name)
    {
        return _species.FirstOrDefault(pokemon => pokemon.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public Pokemon[] GetAllSpecies()
    {
        return _species.ToArray();
    }

    public PokemonMaster[] GetAllMasters()
    {
        return _masters.OrderBy(master => master.Name).ToArray();
    }
}