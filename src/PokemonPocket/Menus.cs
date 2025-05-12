using PokemonPocket.Helpers;
using PokemonPocket.Models;
using Spectre.Console;

namespace PokemonPocket;

internal static class Menus
{
    #region Common Menus

    public static void MainMenu()
    {
        var prompt = new SelectionPrompt<Selection>()
            .Title("Pokemon Pocket")
            .AddChoices(
                "Catch A Pokemon".WithAction(CatchPokemon_Wild),
                "Add A Pokemon".WithAction(AddPokemon_SelectEntity),
                "View My Pokemons".WithAction(ViewPokemons_List),
                "Evolve My Pokemons".WithAction(EvolvePokemon_List),
                "Exit Pocket".WithAction(() => Environment.Exit(0))
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Callback.Invoke();
    }

    #endregion

    #region Catch Pokemons

    public static void CatchPokemon_Wild()
    {
        var entity = Program.Service.Entities[Random.Shared.Next(0, Program.Service.Entities.Count)];

        AnsiConsole.MarkupLineInterpolated($"A wild [bold yellow]{entity.Name}[/] appeared!");
        AnsiConsole.WriteLine();

        var prompt = new SelectionPrompt<Selection>()
            .AddChoices(
                "Catch".WithAction(() => CatchPokemon_Draft(entity)),
                "Run Away".WithAction(CatchPokemon_Wild),
                "Tactical Retreat".WithEmptyAction()
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Callback.Invoke();
    }

    public static void CatchPokemon_Draft(PokemonEntity wildEntity)
    {
        var pets = Program.Service.GetAllPets();
        var groups = pets.ToLookup(pet => pet.EntityId, pet => pet);

        var prompt = new SelectionPrompt<Selection>()
            .Title("Choose Your Pokemon!")
            .PageSize(100)
            .EnableSearch();

        foreach (var group in groups)
        {
            var entity = Program.Service.GetEntity(group.Key);
            if (entity is null) continue;

            var choices = group.Where(pet => pet.Health > 0).Select(pet =>
            {
                var name = pet.GetName();
                var health = pet.Health;
                var experience = pet.Experience;

                return $"{name} (Health: {health}, Experience: {experience})"
                    .WithAction(() => CatchPokemon_Battle(wildEntity, pet));
            });

            prompt.AddChoiceGroup(entity.Name.AsLabel(), choices);
        }

        prompt.AddChoices("Back".WithEmptyAction());

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Callback.Invoke();
    }

    public static void CatchPokemon_Battle(PokemonEntity wildEntity, PokemonPet pet)
    {
        var wild = wildEntity.SpawnPet();
        var petEntity = pet.GetEntity();

        var wildName = wild.GetName();
        var petName = pet.GetName();

        var wildHealth = wild.Health;
        var petHealth = pet.Health;

        var wildDamage = 0;
        var petDamage = 0;

        while (true)
        {
            AnsiConsole.Clear();

            var wildHealthBar = new BarChartItem(wildEntity.Name, wildHealth, Color.Red);
            var petHealthBar = new BarChartItem(pet.GetName(), petHealth, Color.Green);

            var chart = new BarChart()
                .Width(50)
                .AddItem(wildHealthBar)
                .AddItem(petHealthBar);

            AnsiConsole.WriteLine("Battle!");
            AnsiConsole.WriteLine();
            AnsiConsole.Write(chart);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLineInterpolated($"[bold red]{wildName}[/] HP: {wildHealth}");
            AnsiConsole.MarkupLineInterpolated($"[bold green]{petName}[/] HP: {petHealth}");
            AnsiConsole.WriteLine();

            if (petHealth <= 0)
            {
                AnsiConsole.MarkupLineInterpolated($"[bold green]{petName}[/] has fainted!");
                AnsiConsole.WriteLine();

                pet.Health = 0;
                // pet.Experience += wildEntity.Experience;
                Program.Service.SaveDatabase();

                Console.ReadKey();
                break;
            }

            if (wildHealth <= 0)
            {
                AnsiConsole.MarkupLineInterpolated($"You have caught [bold red]{wildName}[/]!");
                AnsiConsole.WriteLine();

                var newPet = new PokemonPet
                {
                    EntityId = wildEntity.Id,
                    Health = 100,
                    Experience = 0
                };

                pet.Health = petHealth;
                // pet.Experience += wildEntity.Experience;
                Program.Service.SaveDatabase();
                Program.Service.AddPet(newPet);

                Console.ReadKey();
                break;
            }

            var prompt = new SelectionPrompt<Selection>()
                .AddChoices(
                    "Attack".WithAction(() =>
                    {
                        petDamage = petEntity.DealDamage();
                        wildDamage = wildEntity.DealDamage();
                    }),
                    "Defend".WithAction(() =>
                    {
                        petDamage = 0;
                        wildDamage = wildEntity.DealDamage() / 50;
                    })
                );

            var result = AnsiConsole.Prompt(prompt).ToAction();
            result.Callback.Invoke();

            AnsiConsole.MarkupLineInterpolated(
                $"You have dealt [bold green]{petDamage}[/] damage to [bold red]{wildEntity.Name}[/]!"
            );

            Thread.Sleep(2000);
            wildHealth -= petDamage;

            AnsiConsole.MarkupLineInterpolated(
                $"[bold red]{wildEntity.Name}[/] has dealt [bold green]{wildDamage}[/] damage to [bold green]{pet.GetName()}[/]!"
            );

            Thread.Sleep(2000);
            petHealth -= wildDamage;
        }
    }

    #endregion

    public static void AddPokemon_SelectEntity()
    {
        var selections = Program.Service.Entities
            .Select(entity => entity.Name.WithAction(() => AddPokemon_SetDetails(entity)));

        var prompt = new SelectionPrompt<Selection>()
            .Title("Pokemon Pocket")
            .AddChoiceGroup("Available Pokemons".AsLabel(), selections)
            .AddChoices("Back".WithEmptyAction())
            .EnableSearch();

        var result = AnsiConsole.Prompt(prompt).ToAction();
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
            if (entity is null) continue;

            var entityName = entity.Name;
            var petName = string.IsNullOrEmpty(pokemon.Name) ? string.Empty : pokemon.Name;
            var health = pokemon.Health;
            var experience = pokemon.Experience;

            table.AddRow(entityName, petName, health.ToString(), experience.ToString());
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
                "Back".WithEmptyAction()
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Callback.Invoke();
    }

    public static void ViewPokemons_Select(Action<PokemonPet> callback)
    {
        AnsiConsole.Clear();

        var pets = Program.Service.GetAllPets();
        var petGroups = pets.ToLookup(pet => pet.EntityId, pet => pet);

        var prompt = new SelectionPrompt<Selection>()
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

            prompt.AddChoiceGroup(groupName.AsLabel(), choices);
        }

        prompt.AddChoices("Back".WithEmptyAction());

        var result = AnsiConsole.Prompt(prompt).ToAction();
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

        var namePrompt = new TextPrompt<string>("Enter Pokemon's New Name: ").AllowEmpty();
        var nameValue = AnsiConsole.Prompt(namePrompt);

        AnsiConsole.Status().Start("Renaming Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Renaming {name} to {nameValue}...");
            Thread.Sleep(5000);

            context.Status("Pokemon renamed successfully!");
            Thread.Sleep(2000);
        });

        pet.Name = nameValue;
        Program.Service.SaveDatabase();
    }

    public static void ViewPokemons_Heal(PokemonPet pet)
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
        Program.Service.SaveDatabase();
    }

    public static void ViewPokemons_Release(PokemonPet pet)
    {
        AnsiConsole.Status().Start("Releasing Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Releasing {pet.Name}...");
            Thread.Sleep(5000);

            context.Status("Pokemon released successfully!");
            Thread.Sleep(2000);
        });

        Program.Service.DeletePet(pet.Id);
    }

    public static void EvolvePokemon_List()
    {
        var pets = Program.Service.GetAllPets();
        var petGroups = pets.ToLookup(pet => pet.EntityId, pet => pet);
        var evolvableEntities = new List<PokemonEntity>();

        var table = new Table();

        table.AddColumn("Name");
        table.AddColumn("Amount");
        table.AddColumn("Required Amount");
        table.AddColumn("Next Evolution");
        table.AddColumn("Evolvable?");

        foreach (var group in petGroups)
        {
            var entity = Program.Service.GetEntity(group.Key);
            if (entity is null) continue;

            var name = entity.Name;
            var amount = group.Count();
            var requiredAmount = "N/A";
            var nextEvolution = "None";
            var evolvable = "No";

            // Check if the entity has a next evolution
            if (entity.NextEvolutionType is not null)
            {
                // Get the next evolution entity
                var nextEvolutionEntity = Program.Service.GetEntity(entity.NextEvolutionType);

                // Update values
                requiredAmount = entity.MinimumEvolutionAmount.ToString();
                nextEvolution = nextEvolutionEntity.Name;

                // Check if current entity is evolvable
                if (amount >= entity.MinimumEvolutionAmount)
                {
                    evolvable = "Yes";
                    evolvableEntities.Add(entity);
                }
            }

            table.AddRow(name, amount.ToString(), requiredAmount, nextEvolution, evolvable);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var choices = evolvableEntities
            .Select(entity => entity.Name.WithAction(() => EvolvePokemon_Select(entity)));

        var prompt = new SelectionPrompt<Selection>()
            .AddChoiceGroup("Eligible Pokemons".AsLabel(), choices)
            .AddChoices("Back".WithEmptyAction());

        var result = AnsiConsole.Prompt(prompt).ToAction();
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

        var prompt = new MultiSelectionPrompt<Selection>()
            .Title("Pokemon Evolution")
            .AddChoices(choices)
            .InstructionsText($"Please select exactly {entity.MinimumEvolutionAmount} pokemon(s) from your pocket.");

        while (true)
        {
            var ids = AnsiConsole.Prompt(prompt).ToValues<Guid>();

            // Continue to prompt if the number of selections is not equal to the required amount
            if (ids.Count != entity.MinimumEvolutionAmount)
                continue;

            evolutionSacrifices = ids.Select(id => Program.Service.GetPet(id)).ToList();

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