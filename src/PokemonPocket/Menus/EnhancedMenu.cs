using PokemonPocket.Helpers;
using PokemonPocket.Models;
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
                "Add A Pokémon".WithAction(AddPokemon_SelectPokemon),
                "View My Pokémons".WithAction(ViewPokemons_ListPokemons),
                "Evolve My Pokémons".WithEmptyAction(),
                "Exit Pocket".WithAction(() => Environment.Exit(0))
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    private static void AddPokemon_SelectPokemon()
    {
        var pokemons = Program.Service.GetAllPokemons();
        var choices = pokemons.Select(pokemon => pokemon.Name.WithAction(() => AddPokemon_SetDetails(pokemon.Name)));

        var prompt = new SelectionPrompt<Selection>()
            .Title("Add A Pokemon")
            .AddChoiceGroup(
                "Available Pokemons".AsLabel(),
                choices.ToArray()
            ).AddChoices(
                "Back To Menu".WithEmptyAction()
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    private static void AddPokemon_SetDetails(string pokemonName)
    {
        var pokemon = Program.Service.GetPokemon(pokemonName);

        AnsiConsole.WriteLine("Pokemon Pocket");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated($"You are about to add [bold yellow]{pokemon.Name}[/]!");
        AnsiConsole.WriteLine();

        var namePrompt = new TextPrompt<string>("Enter Pokemon's Name (optional): ").AllowEmpty();
        var healthPrompt = new TextPrompt<int>("Enter Pokemon's Health: ");
        var experiencePrompt = new TextPrompt<int>("Enter Pokemon's Experience: ");

        var name = AnsiConsole.Prompt(namePrompt);
        var health = AnsiConsole.Prompt(healthPrompt);
        var experience = AnsiConsole.Prompt(experiencePrompt);

        var nameValue = string.IsNullOrEmpty(name) ? null : name;

        AnsiConsole.Status().Start("Locating Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Taming {nameValue ?? pokemon.Name}...");
            Thread.Sleep(5000);

            context.Status("Pokemon caught successfully!");
            Thread.Sleep(2000);
        });

        var pet = pokemon.SpawnPet(nameValue, health, experience);
        Program.Service.AddPet(pet);
    }

    private static void ViewPokemons_ListPokemons()
    {
        var table = new Table();

        table.AddColumn("Pokemon Name");
        table.AddColumn("Given Name");
        table.AddColumn("Health");
        table.AddColumn("Experience");

        var pets = Program.Service.GetAllPets();

        foreach (var pet in pets)
        {
            var pokemonName = pet.Name;
            var petName = string.IsNullOrEmpty(pet.PetName) ? string.Empty : pet.PetName;
            var health = pet.Health;
            var experience = pet.Experience;

            table.AddRow(pokemonName, petName, health.ToString(), experience.ToString());
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var prompt = new SelectionPrompt<Selection>()
            .AddChoiceGroup(
                "Actions".AsLabel(),
                "Rename".WithAction(() => ViewPokemons_Select(ViewPokemons_Rename)),
                "Heal".WithAction(() => ViewPokemons_Select(ViewPokemons_Heal)),
                "Release".WithAction(() => ViewPokemons_Select(ViewPokemons_Release))
            ).AddChoices(
                "Back To Menu".WithEmptyAction()
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    public static void ViewPokemons_Select(Action<Pokemon> callback)
    {
        AnsiConsole.Clear();

        var pets = Program.Service.GetAllPets();
        var petGroups = pets.ToLookup(pet => pet.Name, pet => pet);

        var prompt = new SelectionPrompt<Selection>()
            .PageSize(100)
            .EnableSearch();

        foreach (var group in petGroups)
        {
            var groupName = group.Key;
            var groupChoices = group.Select(pet =>
            {
                var name = string.IsNullOrEmpty(pet.PetName) ? pet.Name : pet.PetName;
                var health = pet.MaxHealth;
                var experience = pet.Experience;

                return $"{name} (Health: {health}, Experience: {experience})"
                    .WithAction(() => callback(pet));
            });

            prompt.AddChoiceGroup(groupName.AsLabel(), groupChoices);
        }

        prompt.AddChoices("Back To Menu".WithEmptyAction());

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    public static void ViewPokemons_Rename(Pokemon pet)
    {
        var pokemon = Program.Service.GetPokemon(pet.Name);
        var petName = string.IsNullOrEmpty(pet.PetName) ? pokemon.Name : pet.PetName;

        AnsiConsole.WriteLine("Pokemon Pocket");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated($"You are about to rename [bold yellow]{petName}[/]!");
        AnsiConsole.WriteLine();

        var namePrompt = new TextPrompt<string>("Enter Pokemon's New Name: ").AllowEmpty();
        var nameValue = AnsiConsole.Prompt(namePrompt);

        AnsiConsole.Status().Start("Renaming Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Renaming {petName} to {nameValue}...");
            Thread.Sleep(5000);

            context.Status("Pokemon renamed successfully!");
            Thread.Sleep(2000);
        });

        pet.PetName = nameValue;
        Program.Service.SaveChanges();
    }

    public static void ViewPokemons_Heal(Pokemon pet)
    {
        AnsiConsole.Status().Start("Healing Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Healing {pet.Name}...");
            Thread.Sleep(5000);

            context.Status("Pokemon healed successfully!");
            Thread.Sleep(2000);
        });

        pet.Health = 100;
        Program.Service.SaveChanges();
    }

    public static void ViewPokemons_Release(Pokemon pet)
    {
        AnsiConsole.Status().Start("Releasing Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Releasing {pet.Name}...");
            Thread.Sleep(5000);

            context.Status("Pokemon released successfully!");
            Thread.Sleep(2000);
        });

        Program.Service.RemovePet(pet);
    }
}