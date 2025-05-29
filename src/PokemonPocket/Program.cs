// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Helpers;
using PokemonPocket.Menus;
using PokemonPocket.Models;
using Spectre.Console;

namespace PokemonPocket;

internal static class Program
{
    public static ProgramService Service { get; } = new();

    public static void Main()
    {
        Console.Clear();
        Console.Title = "Pokemon Pocket";

        var prompt = new SelectionPrompt<Selection>()
            .Title("Please select your mode.")
            .AddChoices(
                "Basic Mode".WithAction(BasicMenu.Start),
                "Enhanced Mode".WithAction(EnhancedMenu.Start)
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();

        while (true)
        {
            AnsiConsole.Clear();
            result.Invoke();
        }
    }
}