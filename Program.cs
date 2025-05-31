// Dennise
// 231292A

using PokemonPocket.Helpers;
using PokemonPocket.Menus;
using Spectre.Console;

namespace PokemonPocket;

internal static class Program
{
    public static ProgramService Service { get; } = new();
    public static ProgramMode Mode { get; set; }

    public static void Main()
    {
        Console.Clear();
        Console.Title = "Pokémon Pocket";

        AnsiConsole.MarkupLine("Welcome to [green]Pokémon Pocket[/]!");
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("Please select a mode to start:");
        AnsiConsole.WriteLine();

        var prompt = new SelectionPrompt<Selection>()
            .AddChoices(
                "Basic Mode".WithAction(() => { Mode = ProgramMode.Basic; }),
                "Enhanced Mode".WithAction(() => { Mode = ProgramMode.Enhanced; })
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();

        while (true)
        {
            AnsiConsole.Clear();
            if (Mode == ProgramMode.Basic)
            {
                BasicMenu.Entry();
            }
            else
            {
                EnhancedMenu.Entry();
            }
        }
    }

    public static void ToggleMenu()
    {
        Mode = Mode == ProgramMode.Basic ? ProgramMode.Enhanced : ProgramMode.Basic;
    }
}