using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PokemonPocket.Models;

namespace PokemonPocket;

public class PokemonService
{
    private static PokemonDatabase _database;

    public IList<PokemonEntity> Entities { get; }

    public PokemonService()
    {
        // Initialize the database
        _database = new PokemonDatabase();

        // Load internal entities
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(PokemonEntity)));
        var entities = types.Select(type => (PokemonEntity)Activator.CreateInstance(type)!).ToList();
        Entities = entities;
    }

    public void AddPet(PokemonPet pokemonPet)
    {
        _database.Pokemon.Add(pokemonPet);
        _database.SaveChanges();
    }

    public IList<PokemonPet> GetAllPets()
    {
        return _database.Pokemon.ToList();
    }

    public IList<PokemonPet> GetPetsByEntity(PokemonEntity entity)
    {
        return _database.Pokemon.Where(pokemon => pokemon.EntityId == entity.Id).ToList();
    }

    public PokemonPet? GetPet(Guid id)
    {
        return _database.Pokemon.Find(id);
    }

    public PokemonEntity? GetEntity(Guid id)
    {
        return Entities.FirstOrDefault(entity => entity.Id == id);
    }

    public PokemonEntity? GetEntity(Type type)
    {
        var obj = Activator.CreateInstance(type);
        if (obj is not PokemonEntity entity) return null;

        return entity;
    }

    public void DeletePet(Guid id)
    {
        var pokemon = _database.Pokemon.FirstOrDefault(pokemon => pokemon.Id == id);
        if (pokemon is null) return;

        _database.Pokemon.Remove(pokemon);
        _database.SaveChanges();
    }

    public void SaveDatabase()
    {
        _database.SaveChanges();
    }
}