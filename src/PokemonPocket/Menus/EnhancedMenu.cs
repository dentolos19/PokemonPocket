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
                "Evolve My Pokémons".WithAction(EvolvePokemon_ListEvolvables),
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
                "Rename".WithAction(() => ViewPokemons_SelectPokemon(ViewPokemons_RenamePokemon)),
                "Heal".WithAction(() => ViewPokemons_SelectPokemon(ViewPokemons_HealPokemon)),
                "Release".WithAction(() => ViewPokemons_SelectPokemon(ViewPokemons_ReleasePokemon))
            ).AddChoices(
                "Back To Menu".WithEmptyAction()
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    public static void ViewPokemons_SelectPokemon(Action<Pokemon> callback)
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

    public static void ViewPokemons_RenamePokemon(Pokemon pet)
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

    public static void ViewPokemons_HealPokemon(Pokemon pet)
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

    public static void ViewPokemons_ReleasePokemon(Pokemon pet)
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

    public static void EvolvePokemon_ListEvolvables()
    {
        var pets = Program.Service.GetAllPets();
        var petGroups = pets.ToLookup(pet => pet.Name, pet => pet);
        var evolvableGroups = petGroups.Where(group => Program.Service.GetMaster(group.Key)?.NoToEvolve > 0);

        var table = new Table();

        table.AddColumn("Name");
        table.AddColumn("Amount");
        table.AddColumn("Evolution");
        table.AddColumn("Evolvable?");

        foreach (var group in petGroups)
        {
            var pokemon = Program.Service.GetPokemon(group.Key);

            if (pokemon is null)
            {
                continue;
            }

            var amount = group.Count().ToString();
            var evolution = "N/A";
            var evolvable = "[red bold]No[/]";

            var master = Program.Service.GetMaster(group.Key);

            if (master is not null)
            {
                amount = $"{group.Count()}/{master.NoToEvolve}";
                evolution = master.EvolveTo;
                evolvable = group.Count() >= master.NoToEvolve ? "[green bold]Yes[/]" : "[red bold]No[/]";
            }

            table.AddRow(pokemon.Name, amount, evolution, evolvable);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var choices = evolvableGroups
            .Select(entity => entity.Key.WithAction(() => EvolvePokemon_SelectSacrifices(entity)));

        var prompt = new SelectionPrompt<Selection>()
            .AddChoiceGroup("Eligible Pokemons".AsLabel(), choices)
            .AddChoices("Back".WithEmptyAction());

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    public static void EvolvePokemon_SelectSacrifices(IGrouping<string, Pokemon> group)
    {
        var master = Program.Service.GetMaster(group.Key);
        var evolution = Program.Service.GetPokemon(master.EvolveTo);
        var sacrifices = new List<Pokemon>();

        var choices = group.Select(pokemon =>
        {
            var name = pokemon.PetName ?? pokemon.Name;
            var health = pokemon.Health;
            var experience = pokemon.Experience;

            return $"{name} (Health: {health}, Experience: {experience})".WithValue(pokemon.Id);
        });

        var prompt = new MultiSelectionPrompt<Selection>()
            .Title("Pokemon Evolution")
            .AddChoices(choices)
            .InstructionsText($"Please select exactly {master.NoToEvolve} pokemon(s) from your pocket.");

        while (true)
        {
            var ids = AnsiConsole.Prompt(prompt).ToValues<string>();

            // Continue to prompt if the number of selections is not equal to the required amount
            if (ids.Count != master.NoToEvolve)
                continue;

            sacrifices = ids.Select(id => group.First(pokemon => pokemon.Id == id)).ToList();
            break;
        }

        AnsiConsole.Status().Start("Evolving Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Evolving {group.Key} with sacrifices...");
            Thread.Sleep(5000);

            context.Status("Pokemon evolved successfully!");
            Thread.Sleep(2000);
        });

        foreach (var sacrifice in sacrifices)
        {
            Program.Service.RemovePet(sacrifice);
        }

        var pet = evolution.SpawnPet(null, 100, 0);
        Program.Service.AddPet(pet);
    }
}