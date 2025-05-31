// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Helpers;
using PokemonPocket.Models;
using Spectre.Console;

namespace PokemonPocket.Menus;

public static class EnhancedMenu
{
    public static void Entry()
    {
        var figlet = new FigletText("Pokémon Pocket");
        AnsiConsole.Write(figlet);

        var prompt = new SelectionPrompt<Selection>()
            .AddChoiceGroup(
                "Actions".AsLabel(),
                "Catch A Pokémon".WithAction(CatchPokemon_Wilding),
                "Add A Pokémon".WithAction(AddPokemon_SelectPokemon),
                "View My Pokémons".WithAction(ViewPokemons_ListPokemons),
                "Evolve My Pokémons".WithAction(EvolvePokemon_ListEvolvables)
            )
            .AddChoiceGroup(
                "Pocket".AsLabel(),
                "Switch To Basic Menu".WithAction(() => { Program.ToggleMenu(); }),
                "Exit My Pocket".WithAction(() => { Environment.Exit(0); })
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
                "Target Pokemon".WithAction(() => CatchPokemon_Draft(randomPokemon)),
                "Move On".WithAction(CatchPokemon_Wilding)
            ).AddChoices(
                "Back To Base".WithEmptyAction()
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    private static void CatchPokemon_Draft(Pokemon wild)
    {
        var pets = Program.Service.GetAllPets();
        var groups = pets.ToLookup(pet => pet.Name, pet => pet);

        var prompt = new SelectionPrompt<Selection>()
            .PageSize(100)
            .EnableSearch();

        foreach (var group in groups)
        {
            var availablePets = group.Where(pet => pet.Health > 0).ToArray();
            if (availablePets.Length <= 0)
                continue;

            var choices = availablePets.Select(draft =>
            {
                var name = string.IsNullOrEmpty(draft.PetName) ? draft.Name : draft.PetName;
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

            // Render health bars
            var chart = new BarChart()
                .Width(50)
                .WithMaxValue(100)
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
                var gainedExperience = Random.Shared.Next(10, 30);

                AnsiConsole.MarkupLineInterpolated($"Both have fainted!");
                AnsiConsole.MarkupLineInterpolated(
                    $"However, [green]{draftName}[/] has gained [green]{gainedExperience}[/] experience!"
                );

                AnsiConsole.WriteLine();

                draft.Health = 0;
                draft.Experience += gainedExperience;
                Program.Service.SaveChanges();

                Console.ReadKey();
                break;
            }

            // If the draft is defeated...
            if (draftHealth <= 0)
            {
                var gainedExperience = Random.Shared.Next(10, 30);

                AnsiConsole.MarkupLineInterpolated($"[green]{draftName}[/] has fainted!");
                AnsiConsole.MarkupLineInterpolated(
                    $"However, [green]{draftName}[/] has gained [green]{gainedExperience}[/] experience!"
                );

                AnsiConsole.WriteLine();

                draft.Health = 0;
                draft.Experience += gainedExperience;
                Program.Service.SaveChanges();

                Console.ReadKey();
                break;
            }

            // If the wild is defeated...
            if (wildHealth <= 0)
            {
                var gainedExperience = Random.Shared.Next(20, 50);

                AnsiConsole.MarkupLineInterpolated($"You have caught [bold red]{wildName}[/]!");
                AnsiConsole.MarkupLineInterpolated(
                    $"[green]{draftName}[/] has gained [green]{gainedExperience}[/] experience!"
                );

                AnsiConsole.WriteLine();

                draft.Health = draftHealth;
                draft.Experience += gainedExperience;

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

            Thread.Sleep(1000);
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
            .PageSize(100)
            .EnableSearch()
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

        AnsiConsole.MarkupLineInterpolated($"You are about to add [bold yellow]{pokemon.Name}[/]!");
        AnsiConsole.WriteLine();

        var namePrompt = new TextPrompt<string>("Enter Pokemon's Name (optional): ").AllowEmpty();
        var healthPrompt = new TextPrompt<int>("Enter Pokemon's Health: ")
            .Validate(value =>
                value >= 0 ? ValidationResult.Success() : ValidationResult.Error("Value must be non-negative."));
        var experiencePrompt = new TextPrompt<int>("Enter Pokemon's Experience: ")
            .Validate(value =>
                value >= 0 ? ValidationResult.Success() : ValidationResult.Error("Value must be non-negative."));

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
            var health = $"{pet.Health}/{pet.MaxHealth}";
            var experience = pet.Experience;

            if (pet.Health >= pet.MaxHealth - 10)
                health = $"[green]{health}[/]";
            else if (pet.Health <= 10)
                health = $"[red]{health}[/]";
            else
                health = $"[yellow]{health}[/]";

            table.AddRow(pokemonName, petName, health, experience.ToString());
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
            .PageSize(30)
            .EnableSearch();

        foreach (var group in petGroups)
        {
            var groupName = group.Key;
            var groupChoices = group.Select(pet =>
            {
                var name = pet.GetName();
                var health = $"{pet.Health}/{pet.MaxHealth}";
                var experience = pet.Experience;

                if (pet.Health >= pet.MaxHealth - 10)
                    health = $"[green]{health}[/]";
                else if (pet.Health <= 10)
                    health = $"[red]{health}[/]";
                else
                    health = $"[yellow]{health}[/]";

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

        AnsiConsole.WriteLine("Pokémon Pocket");
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
        var name = pet.GetName();

        var healthRequired = pet.MaxHealth - pet.Health;
        var healthIncreasable = pet.Experience > healthRequired ? healthRequired : pet.Experience;
        var outputHealth = pet.Health + healthIncreasable;

        if (healthRequired <= 0)
        {
            AnsiConsole.MarkupLineInterpolated($"[yellow]{name}[/] is already at full health!");
            Console.ReadKey(true);
            return;
        }

        if (healthIncreasable <= 0)
        {
            AnsiConsole.MarkupLineInterpolated($"[yellow]{name}[/] does not have enough experience to heal!");
            Console.ReadKey(true);
            return;
        }

        AnsiConsole.WriteLine("Pokémon Pocket");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated(
            $"You are about to heal [yellow]{name}[/] from [red]{pet.Health}[/] to [green]{outputHealth}[/]!");
        AnsiConsole.MarkupLineInterpolated($"This will consume [red]{healthIncreasable}[/] experience points.");
        AnsiConsole.WriteLine();

        var confirmation = AnsiConsole.Confirm("Do you want to proceed?");
        if (!confirmation)
            return;

        AnsiConsole.Status().Start("Healing Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Healing {pet.Name}...");
            Thread.Sleep(5000);

            context.Status("Pokemon healed successfully!");
            Thread.Sleep(2000);
        });

        pet.Health = outputHealth;
        pet.Experience -= healthIncreasable;
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

        var masters = Program.Service.GetAllMasters();
        var eligibleMasters = new List<PokemonMaster>();

        var table = new Table();

        table.AddColumn("From");
        table.AddColumn("To");
        table.AddColumn("Amount");
        table.AddColumn("Evolvable?");

        foreach (var master in masters)
        {
            var from = master.Name;
            var to = master.EvolveTo;

            var fromNumber = petGroups.Contains(from) ? petGroups[from].Count() : 0;
            var requiredAmount = master.NoToEvolve;

            if (fromNumber <= 0)
                continue;

            var amount = $"{fromNumber}/{requiredAmount}";
            var evolvable = "[red]No[/]";

            if (master.CanEvolve(pets))
            {
                evolvable = "[green]Yes[/]";
                eligibleMasters.Add(master);
            }

            table.AddRow(from, to, amount, evolvable);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        if (eligibleMasters.Count > 0)
        {
            var prompt = new SelectionPrompt<Selection>()
                .AddChoiceGroup(
                    "Evolvable Masters".AsLabel(),
                    eligibleMasters.Select(master =>
                        $"{master.Name} -> {master.EvolveTo}".WithAction(() => EvolvePokemon_SelectSacrifices(master))
                    ))
                .AddChoices(
                    "Back To Menu".WithEmptyAction()
                );

            var result = AnsiConsole.Prompt(prompt).ToAction();

            AnsiConsole.Clear();
            result.Invoke();
        }
        else
        {
            AnsiConsole.MarkupLine("[gray]No evolvable pokemons found.[/]");
            Console.ReadKey(true);
        }
    }

    public static void EvolvePokemon_SelectSacrifices(PokemonMaster master)
    {
        var candidates = Program.Service.GetPokemonPets(master.Name);
        var evolution = Program.Service.GetPokemon(master.EvolveTo);
        var sacrifices = new List<Pokemon>();
        var output = 0;

        var choices = candidates.Select(pokemon =>
        {
            var name = pokemon.PetName ?? pokemon.Name;
            var health = pokemon.Health;
            var experience = pokemon.Experience;

            return $"{name} (Health: {health}, Experience: {experience})".WithValue(pokemon.Id);
        });

        AnsiConsole.WriteLine("Pokémon Pocket");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated(
            $"You are about to evolve from [yellow]{master.Name}[/] to [yellow]{master.EvolveTo}[/]!");
        AnsiConsole.WriteLine();

        var prompt = new MultiSelectionPrompt<Selection>()
            .AddChoices(choices)
            .InstructionsText(
                $"Please select exactly or multiples of {master.NoToEvolve} pokemon(s) from your pocket.");

        while (true)
        {
            var ids = AnsiConsole.Prompt(prompt).ToValues<string>();

            if (ids.Count < master.NoToEvolve || ids.Count % master.NoToEvolve != 0)
                continue;

            sacrifices = ids.Select(id => candidates.First(pokemon => pokemon.Id == id)).ToList();
            output = ids.Count / master.NoToEvolve;
            break;
        }

        AnsiConsole.Status().Start("Evolving Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Sacrificing {sacrifices.Count} {master.Name} to evolve to {output} {master.EvolveTo}...");
            Thread.Sleep(5000);

            context.Status("Pokemon evolved successfully!");
            Thread.Sleep(2000);
        });

        foreach (var sacrifice in sacrifices)
        {
            Program.Service.RemovePet(sacrifice);
        }

        var pet = evolution.SpawnPet();
        Program.Service.AddPet(pet);
    }
}