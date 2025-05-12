using PokemonPocket.Helpers;
using PokemonPocket.Models;
using Spectre.Console;

namespace PokemonPocket;

internal sealed class Menus
{
    public static void MainMenu()
    {
        var prompt = new SelectionPrompt<SelectionAction>()
            .Title("Pokemon Pocket")
            .AddChoices(
                "Add A Pokemon".WithAction(AddPokemon_SelectEntity),
                "View My Pokemons".WithAction(ViewPokemons_List),
                "Evolve My Pokemons".WithAction(EvolvePokemon_List),
                "Exit Pocket".WithAction(() => Environment.Exit(0))
            );

        var result = AnsiConsole.Prompt(prompt);
        result.Callback.Invoke();
    }

    public static void AddPokemon_SelectEntity()
    {
        var selections = Program.Service.Entities
            .Select(entity => entity.Name.WithAction(() => AddPokemon_SetDetails(entity)));

        var prompt = new SelectionPrompt<SelectionAction>()
            .Title("Pokemon Pocket")
            .AddChoiceGroup("Available Pokemons".WithEmptyAction(), selections)
            .AddChoices("Back".WithEmptyAction())
            .EnableSearch();

        var result = AnsiConsole.Prompt(prompt);
        result.Callback.Invoke();
    }

    public static void AddPokemon_SetDetails(PokemonEntity entity)
    {
        AnsiConsole.WriteLine("Pokemon Pocket");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated($"You are about to add [bold yellow]{entity.Name}[/]!");
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

            context.Status($"Taming {nameValue ?? entity.Name}...");
            Thread.Sleep(5000);

            context.Status("Pokemon caught successfully!");
            Thread.Sleep(2000);
        });

        var pet = new PokemonPet
        {
            EntityId = entity.Id,
            Name = nameValue,
            Health = health,
            Experience = experience
        };

        Program.Service.AddPet(pet);
    }

    public static void ViewPokemons_List()
    {
        var table = new Table();

        table.AddColumn("Pokemon Name");
        table.AddColumn("Given Name");
        table.AddColumn("Health");
        table.AddColumn("Experience");

        var pets = Program.Service.GetAllPets();

        foreach (var pokemon in pets)
        {
            var entity = pokemon.GetEntity();

            var entityName = entity.Name;
            var petName = string.IsNullOrEmpty(pokemon.Name) ? string.Empty : pokemon.Name;
            var health = pokemon.Health;
            var experience = pokemon.Experience;

            table.AddRow(entityName, petName, health.ToString(), experience.ToString());
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var prompt = new SelectionPrompt<SelectionAction>()
            .AddChoiceGroup(
                "Actions".WithEmptyAction(),
                "Rename".WithAction(() => ViewPokemons_Select(ViewPokemons_Rename)),
                "Heal".WithEmptyAction(),
                "Release".WithEmptyAction()
            ).AddChoices(
                "Back".WithEmptyAction()
            );

        var result = AnsiConsole.Prompt(prompt);
        result.Callback.Invoke();
    }

    public static void ViewPokemons_Select(Action<PokemonPet> callback)
    {
        AnsiConsole.Clear();

        var pets = Program.Service.GetAllPets();
        var petGroups = pets.ToLookup(pet => pet.EntityId, pet => pet);

        var prompt = new SelectionPrompt<SelectionAction>()
            .PageSize(100)
            .EnableSearch();

        foreach (var group in petGroups)
        {
            var entity = Program.Service.GetEntity(group.Key);
            if (entity is null) continue;

            var groupName = entity.Name;

            var choices = group.Select(pokemon =>
            {
                var name = string.IsNullOrEmpty(pokemon.Name) ? entity.Name : pokemon.Name;
                var health = pokemon.Health;
                var experience = pokemon.Experience;

                return $"{name} (Health: {health}, Experience: {experience})"
                    .WithAction(() => callback(pokemon));
            });

            prompt.AddChoiceGroup(groupName.WithEmptyAction(), choices);
        }

        prompt.AddChoices("Back".WithEmptyAction());

        var result = AnsiConsole.Prompt(prompt);
        result.Callback.Invoke();
    }

    public static void ViewPokemons_Rename(PokemonPet pet)
    {
        var entity = pet.GetEntity();
        var name = string.IsNullOrEmpty(pet.Name) ? entity.Name : pet.Name;

        AnsiConsole.WriteLine("Pokemon Pocket");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated($"You are about to rename [bold yellow]{name}[/]!");
        AnsiConsole.WriteLine();

        var namePrompt = new TextPrompt<string>("Enter Pokemon's Name: ").AllowEmpty();
        var nameValue = AnsiConsole.Prompt(namePrompt);

        if (string.IsNullOrEmpty(nameValue))
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Pokemon's name cannot be empty![/]");
            return;
        }

        AnsiConsole.Status().Start("Renaming Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Renaming {name}...");
            Thread.Sleep(5000);

            context.Status("Pokemon renamed successfully!");
            Thread.Sleep(2000);
        });

        pet.Name = nameValue;
        Program.Service.SaveDatabase();
    }

    public static void EvolvePokemon_List()
    {
        var pets = Program.Service.GetAllPets();
        var petGroups = pets.ToLookup(pet => pet.EntityId, pet => pet);
        var evolvableGroups = new List<PokemonEntity>();

        var table = new Table();

        table.AddColumn("Name");
        table.AddColumn("Amount");
        table.AddColumn("Evolution Amount");
        table.AddColumn("Next Evolution");
        table.AddColumn("Evolvable?");

        foreach (var group in petGroups)
        {
            var entity = Program.Service.GetEntity(group.Key);
            if (entity is null) continue;

            // Set default values
            var name = entity.Name;
            var amount = group.Count();
            var evolutionAmount = "N/A";
            var nextEvolution = "None";
            var evolvable = "No";

            // Check if the entity has a next evolution
            if (entity.NextEvolutionType is not null)
            {
                // Get the next evolution entity
                var nextEvolutionEntity = Program.Service.GetEntity(entity.NextEvolutionType);

                // Update values
                evolutionAmount = entity.MinimumEvolutionAmount.ToString();
                nextEvolution = nextEvolutionEntity.Name;

                // Check if current entity is evolvable
                if (amount >= entity.MinimumEvolutionAmount)
                {
                    evolvable = "Yes";
                    evolvableGroups.Add(entity);
                }
            }

            table.AddRow(name, amount.ToString(), evolutionAmount, nextEvolution, evolvable);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var choices = evolvableGroups
            .Select(entity => entity.Name.WithAction(() => EvolvePokemon_Select(entity)));

        var prompt = new SelectionPrompt<SelectionAction>()
            .AddChoiceGroup("Eligible Pokemons".WithEmptyAction(), choices)
            .AddChoices("Back".WithEmptyAction());

        var result = AnsiConsole.Prompt(prompt);
        result.Callback.Invoke();
    }

    public static void EvolvePokemon_Select(PokemonEntity entity)
    {
        AnsiConsole.Clear();

        var sacrificeCandidates = Program.Service.GetPetsByEntity(entity);
        var evolutionSacrifices = new List<PokemonPet>();

        var choices = sacrificeCandidates.Select(pokemon =>
        {
            var name = pokemon.Name ?? entity.Name;
            var health = pokemon.Health;
            var experience = pokemon.Experience;

            return $"{name} (Health: {health}, Experience: {experience})".WithValue(pokemon.Id);
        });

        var prompt = new MultiSelectionPrompt<SelectionValue<Guid>>()
            .Title("Pokemon Evolution")
            .AddChoices(choices)
            .InstructionsText($"Please select exactly {entity.MinimumEvolutionAmount} pokemon(s) from your pocket.");

        while (true)
        {
            var selections = AnsiConsole.Prompt(prompt);
            if (selections.Count != entity.MinimumEvolutionAmount)
                continue;

            evolutionSacrifices = selections.Select(selection => Program.Service.GetPet(selection.Value)).ToList();
            break;
        }

        EvolvePokemon_Process(entity, evolutionSacrifices.ToArray());
    }

    public static void EvolvePokemon_Process(PokemonEntity entity, PokemonPet[] sacrifices)
    {
        var evolutionEntity = Program.Service.GetEntity(entity.NextEvolutionType);

        AnsiConsole.Status().Start("Catching Pokemons...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Evolving to {evolutionEntity.Name}...");
            Thread.Sleep(5000);

            context.Status("Evolved successfully!");
            Thread.Sleep(2000);
        });

        foreach (var sacrifice in sacrifices)
        {
            Program.Service.DeletePet(sacrifice.Id);
        }

        var newPokemon = new PokemonPet
        {
            EntityId = evolutionEntity.Id,
            Health = 100,
            Experience = 0
        };

        Program.Service.AddPet(newPokemon);
    }
}