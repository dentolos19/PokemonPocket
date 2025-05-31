// Catolos Alvaro Dennise Jay San Juan
// 231292A

using Microsoft.EntityFrameworkCore;
using PokemonPocket.Models;

namespace PokemonPocket;

public class ProgramDatabase : DbContext
{
    private readonly string _databasePath;

    public DbSet<Pokemon> Pokemons { get; init; }

    public ProgramDatabase()
    {
        // Configure Database Path
        var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var databasePath = Path.Join(currentDirectory, "pocket.db");

        _databasePath = databasePath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_databasePath}");
    }
}