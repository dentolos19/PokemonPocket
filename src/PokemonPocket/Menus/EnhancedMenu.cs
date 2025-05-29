// Catolos Alvaro Dennise Jay San Juan
// 231292A

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
                "Catch A Pokémon".WithAction(CatchPokemon_Wilding),
                "Add A Pokémon".WithAction(AddPokemon_SelectPokemon),
                "View My Pokémons".WithAction(ViewPokemons_ListPokemons),
                "Evolve My Pokémons".WithAction(EvolvePokemon_ListEvolvables),
                "Exit Pocket".WithAction(() => Environment.Exit(0))
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    private static void CatchPokemon_Wilding()
    {
        AnsiConsole.Clear();

        var availablePokemons = Program.Service.GetAllPokemons();
        var randomPokemon = availablePokemons.OrderBy(_ => Guid.NewGuid()).First().SpawnPet();

        AnsiConsole.MarkupLineInterpolated($"A wild [yellow bold]{randomPokemon.Name}[/] has been spotted!");
        AnsiConsole.WriteLine();

        var prompt = new SelectionPrompt<Selection>()
            .AddChoiceGroup(
                "Actions".AsLabel(),
                "Target".WithAction(() => CatchPokemon_Draft(randomPokemon)),
                "Move On".WithAction(CatchPokemon_Wilding)
            ).AddChoices(
                "Back To Menu".WithEmptyAction()
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    private static void CatchPokemon_Draft(Pokemon wild)
    {
        var pets = Program.Service.GetAllPets();
        var groups = pets.ToLookup(pet => pet.Name, pet => pet);

        var prompt = new SelectionPrompt<Selection>()
            .Title("Choose Your Pokemon!")
            .PageSize(100)
            .EnableSearch();

        foreach (var group in groups)
        {
            var choices = group.Where(pet => pet.Health > 0).Select(draft =>
            {
                var name = draft.PetName;
                var health = draft.Health;
                var experience = draft.Experience;

                return $"{name} (Health: {health}, Experience: {experience})"
                    .WithAction(() => CatchPokemon_Catch(wild, draft));
            });

            prompt.AddChoiceGroup(group.Key.AsLabel(), choices);
        }

        prompt.AddChoices("Back".WithEmptyAction());

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    private static void CatchPokemon_Catch(Pokemon wild, Pokemon draft)
    {
        // Declare the wild's and draft's names
        var wildName = wild.Name;
        var draftName = string.IsNullOrEmpty(draft.PetName) ? draft.Name : draft.PetName;

        // Declare the wild's and draft's entities
        var wildHealth = wild.Health;
        var draftHealth = draft.Health;

        while (true)
        {
            AnsiConsole.Clear();

            var wildHealthBar = new BarChartItem(wildName, wildHealth, Color.Red);
            var petHealthBar = new BarChartItem(draftName, draftHealth, Color.Green);

            var chart = new BarChart()
                .Width(50)
                .AddItem(wildHealthBar)
                .AddItem(petHealthBar);

            AnsiConsole.WriteLine("Battle!");
            AnsiConsole.WriteLine();
            AnsiConsole.Write(chart);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLineInterpolated($"[bold red]{wildName}[/] HP: {wildHealth}");
            AnsiConsole.MarkupLineInterpolated($"[bold green]{draftName}[/] HP: {draftHealth}");
            AnsiConsole.WriteLine();

            // If both are defeated...
            if (draftHealth <= 0 && wildHealth <= 0)
            {
                AnsiConsole.MarkupLineInterpolated($"Both have fainted!" );
                AnsiConsole.WriteLine();

                draft.Health = 0;
                wild.Health = 0;
                Program.Service.SaveChanges();

                Console.ReadKey();
                break;
            }

            // If the draft is defeated...
            if (draftHealth <= 0)
            {
                AnsiConsole.MarkupLineInterpolated($"[bold green]{draftName}[/] has fainted!");
                AnsiConsole.WriteLine();

                draft.Health = 0;
                // pet.Experience += wildEntity.Experience;
                Program.Service.SaveChanges();

                Console.ReadKey();
                break;
            }

            // If the wild is defeated...
            if (wildHealth <= 0)
            {
                AnsiConsole.MarkupLineInterpolated($"You have caught [bold red]{wildName}[/]!");
                AnsiConsole.WriteLine();

                draft.Health = draftHealth;
                // pet.Experience += wildEntity.Experience;

                var pet = wild.SpawnPet();
                Program.Service.AddPet(pet);
                Program.Service.SaveChanges();

                Console.ReadKey();
                break;
            }

            var wildDamage = 0;
            var petDamage = 0;

            var prompt = new SelectionPrompt<Selection>()
                .AddChoiceGroup(
                    "Actions".AsLabel(),
                    "Attack".WithAction(() =>
                    {
                        wildDamage = wild.CalculateDamage(draft.DealDamage());
                        petDamage = draft.CalculateDamage(wild.DealDamage());
                    })
                );

            var result = AnsiConsole.Prompt(prompt).ToAction();
            result.Invoke();

            AnsiConsole.MarkupLineInterpolated(
                $"[bold green]{draftName}[/] have dealt [bold green]{petDamage}[/] damage to [bold red]{wildName}[/]!"
            );

            Thread.Sleep(2000);
            wildHealth -= petDamage;

            AnsiConsole.MarkupLineInterpolated(
                $"[bold red]{wildName}[/] has dealt [bold green]{wildDamage}[/] damage to [bold green]{draftName}[/]!"
            );

            Thread.Sleep(2000);
            draftHealth -= wildDamage;
        }
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
        // Retrieve all pokemon owned by the user
        var pets = Program.Service.GetAllPets();

        // Group the pokemons by their type
        var petGroups = pets.ToLookup(pet => pet.Name, pet => pet);

        // Filter groups that can be evolved
        var evolvableGroups =
            petGroups.Where(group => group.Count() > Program.Service.GetMaster(group.Key)?.NoToEvolve);

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
            .Select(entity => entity.Key.WithAction(() => EvolvePokemon_SelectSacrifices(entity))).ToArray();

        var prompt = new SelectionPrompt<Selection>();

        if (choices.Length > 0)
        {
            prompt.AddChoiceGroup("Evolve Pokemon".AsLabel(), choices);
        }

        prompt = prompt.AddChoices("Back".WithEmptyAction());

        var result = AnsiConsole.Prompt(prompt).ToAction();
        AnsiConsole.Clear();
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