using Microsoft.EntityFrameworkCore;
using PokemonPocket.Models;

namespace PokemonPocket;

public class PokemonDatabase : DbContext
{
    private static string _databasePath;

    public DbSet<PokemonPet> Pokemon { get; set; }

    public PokemonDatabase()
    {
        var directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var databasePath = Path.Join(directoryPath, "pokemon.db");
        _databasePath = databasePath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_databasePath}");
    }
}