using PokemonPocket.Helpers;
using Spectre.Console;

namespace PokemonPocket.Menus;

public static class EnhancedMenu
{
    public static void Start()
    {
        var prompt = new SelectionPrompt<Selection>()
            .Title("Pokémon Pocket")
            .AddChoices(
                "Catch A Pokémon".WithEmptyAction(),
                "Add A Pokémon".WithEmptyAction(),
                "View My Pokémons".WithEmptyAction(),
                "Evolve My Pokémons".WithEmptyAction(),
                "Exit Pocket".WithAction(() => Environment.Exit(0))
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }
}