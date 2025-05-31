// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Helpers;
using PokemonPocket.Menus;
using PokemonPocket.Models;
using Spectre.Console;

namespace PokemonPocket;

internal static class Program
{
    private delegate void Menu();

    private static Menu _menu = BasicMenu.Entry;

    public static ProgramService Service { get; } = new();

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
                "Basic Mode".WithAction(() => { _menu = BasicMenu.Entry; }),
                "Enhanced Mode".WithAction(() => { _menu = EnhancedMenu.Entry; })
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();

        while (true)
        {
            AnsiConsole.Clear();
            _menu.Invoke();
        }
    }

    public static void ToggleMenu()
    {
        if (_menu == BasicMenu.Entry)
        {
            _menu = EnhancedMenu.Entry;
        }
        else
        {
            _menu = BasicMenu.Entry;
        }
    }
}