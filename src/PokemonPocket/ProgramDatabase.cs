using Microsoft.EntityFrameworkCore;
using PokemonPocket.Models;

namespace PokemonPocket;

public class ProgramDatabase : DbContext
{
    private readonly string _databasePath;

    public DbSet<Pokemon> Pets { get; set; }


    public ProgramDatabase()
    {
        // Configure Database Path
        var directoryPath = AppDomain.CurrentDomain.BaseDirectory;
        var databasePath = Path.Join(directoryPath, "pocket.db");

        _databasePath = databasePath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_databasePath}");
    }
}