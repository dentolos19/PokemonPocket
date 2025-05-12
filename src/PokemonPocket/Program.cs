using Spectre.Console;

namespace PokemonPocket;

internal static class Program
{
    public static PokemonService Service = new();

    public static void Main()
    {
        while (true)
        {
            AnsiConsole.Clear();
            Menus.MainMenu();
        }
    }
}