// Dennise
// 231292A

using PokemonPocket.Helpers;
using PokemonPocket.Models;
using Spectre.Console;

namespace PokemonPocket.Menus;

public static class EnhancedMenu
{
    public static void Entry()
    {
        var title = new FigletText("Pokémon Pocket").Color(Color.Yellow).Centered();
        var subtitle = new Rule("Welcome to your Pocket Adventure!").RuleStyle(new Style(Color.Green));

        AnsiConsole.Write(title);
        AnsiConsole.Write(subtitle);
        AnsiConsole.WriteLine();

        var prompt = new SelectionPrompt<Selection>()
            .AddChoiceGroup(
                "My Adventure".AsLabel(),
                "Catch A Pokémon".WithAction(CatchPokemon_Wilding),
                "Add A Pokémon".WithAction(AddPokemon_SelectPokemon),
                "View My Pokémons".WithAction(ViewPokemons_ListPokemons),
                "Evolve My Pokémons".WithAction(EvolvePokemon_ListEvolvables)
            )
            .AddChoiceGroup(
                "My Pocket".AsLabel(),
                "Switch To Basic Menu".WithAction(() => { Program.ToggleMenu(); }),
                "Exit My Pocket".WithAction(() => { Environment.Exit(0); })
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    private static void CatchPokemon_Wilding()
    {
        AnsiConsole.Clear();

        var title = new FigletText("Pokémon Pocket").Color(Color.Yellow).Centered();
        var subtitle = new Rule("Battle Preparation!").RuleStyle(new Style(Color.Orange1));

        AnsiConsole.Write(title);
        AnsiConsole.Write(subtitle);
        AnsiConsole.WriteLine();

        var availableSpecies = Program.Service.GetAllSpecies();
        var randomSpecies = availableSpecies.OrderBy(_ => Guid.NewGuid()).First().SpawnPokemon();

        AnsiConsole.MarkupLineInterpolated($"A wild [yellow]{randomSpecies.Name}[/] has been spotted!");
        AnsiConsole.WriteLine();

        var prompt = new SelectionPrompt<Selection>()
            .AddChoices(
                "Target Pokemon".WithAction(() => CatchPokemon_Draft(randomSpecies)),
                "Move On".WithAction(CatchPokemon_Wilding),
                "Back".WithEmptyAction()
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    private static void CatchPokemon_Draft(Pokemon wild)
    {
        var pokemons = Program.Service.GetAllPokemons();
        var groups = pokemons.ToLookup(pet => pet.Name, pet => pet);

        var prompt = new SelectionPrompt<Selection>()
            .PageSize(30)
            .EnableSearch();

        foreach (var group in groups)
        {
            var groupPokemons = group.Where(pet => pet.Health > 0).ToArray();
            if (groupPokemons.Length <= 0)
                continue;

            var choices = groupPokemons.Select(draft =>
            {
                var name = draft.GetName();
                var health = draft.Health;
                var experience = draft.Experience;

                return $"{name} (Health: {health}, Experience: {experience})".WithAction(() => CatchPokemon_Catch(wild, draft));
            });

            prompt.AddChoiceGroup(group.Key.AsLabel(), choices);
        }

        prompt.AddChoices("Back".WithEmptyAction());

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    private static void CatchPokemon_Catch(Pokemon wild, Pokemon draft)
    {
        var title = new FigletText("Pokémon Pocket").Color(Color.Yellow).Centered();
        var subtitle = new Rule("Battle Mode!").RuleStyle(new Style(Color.Red));

        // Declare the wild's and draft's names
        var wildName = wild.Name;
        var draftName = draft.GetName();

        // Declare the wild's and draft's entities
        var wildHealth = wild.Health;
        var draftHealth = draft.Health;

        var playing = true;

        while (playing)
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

            AnsiConsole.Write(title);
            AnsiConsole.Write(subtitle);
            AnsiConsole.WriteLine();
            AnsiConsole.Write(chart);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLineInterpolated($"[green]{draftName}[/] HP: {draftHealth}");
            AnsiConsole.MarkupLineInterpolated($"[red]{wildName}[/] HP: {wildHealth}");
            AnsiConsole.WriteLine();

            // If both are defeated...
            if (draftHealth <= 0 && wildHealth <= 0)
            {
                var gainedExperience = Random.Shared.Next(10, 30);

                AnsiConsole.MarkupLineInterpolated($"Both have fainted!");
                AnsiConsole.MarkupLineInterpolated($"However, [green]{draftName}[/] has gained [yellow]{gainedExperience}[/] experience!");
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
                AnsiConsole.MarkupLineInterpolated($"However, [green]{draftName}[/] has gained [yellow]{gainedExperience}[/] experience!");
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
                AnsiConsole.MarkupLineInterpolated($"[green]{draftName}[/] has gained [yellow]{gainedExperience}[/] experience!");

                AnsiConsole.WriteLine();

                draft.Health = draftHealth;
                draft.Experience += gainedExperience;

                var pokemon = wild.SpawnPokemon();
                Program.Service.AddPokemon(pokemon);
                Program.Service.SaveChanges();

                Console.ReadKey();
                break;
            }

            var prompt = new SelectionPrompt<Selection>()
                .AddChoices(
                    "Attack".WithAction(() =>
                    {
                        var draftDamage = draft.CalculateDamage(wild.DealDamage());
                        var wildDamage = wild.CalculateDamage(draft.DealDamage());

                        AnsiConsole.MarkupLineInterpolated($"[green]{draftName}[/] have dealt [yellow]{draftDamage}[/] damage to [red]{wildName}[/]!");
                        Thread.Sleep(1000);
                        wildHealth -= draftDamage;

                        AnsiConsole.MarkupLineInterpolated($"[red]{wildName}[/] has dealt [yellow]{wildDamage}[/] damage to [green]{draftName}[/]!");
                        Thread.Sleep(2000);
                        draftHealth -= wildDamage;
                    }),
                    "Retreat".WithAction(() =>
                    {
                        playing = false;
                    })
                );

            var result = AnsiConsole.Prompt(prompt).ToAction();
            result.Invoke();
        }
    }

    private static void AddPokemon_SelectPokemon()
    {
        var pokemons = Program.Service.GetAllSpecies();
        var choices = pokemons.Select(pokemon => pokemon.Name.WithAction(() => AddPokemon_SetDetails(pokemon.Name)));

        var prompt = new SelectionPrompt<Selection>()
            .PageSize(100)
            .EnableSearch()
            .AddChoiceGroup(
                "Available Species".AsLabel(),
                choices.ToArray()
            ).AddChoices(
                "Back".WithEmptyAction()
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    private static void AddPokemon_SetDetails(string speciesName)
    {
        var species = Program.Service.GetSpecies(speciesName);

        AnsiConsole.MarkupLineInterpolated($"You are about to add [bold yellow]{species.Name}[/]!");
        AnsiConsole.WriteLine();

        var namePrompt = new TextPrompt<string>("Enter Pokemon's Name (optional): ").AllowEmpty();
        var healthPrompt = new TextPrompt<int>("Enter Pokemon's Health: ")
            .Validate(value => value >= 0 ? ValidationResult.Success() : ValidationResult.Error("Value must be non-negative."))
            .Validate(value => value > species.MaxHealth ? ValidationResult.Error($"Value must be less than or equal to {species.MaxHealth}.") : ValidationResult.Success());
        var experiencePrompt = new TextPrompt<int>("Enter Pokemon's Experience: ")
            .Validate(value => value >= 0 ? ValidationResult.Success() : ValidationResult.Error("Value must be non-negative."));

        var name = AnsiConsole.Prompt(namePrompt);
        var health = AnsiConsole.Prompt(healthPrompt);
        var experience = AnsiConsole.Prompt(experiencePrompt);

        var pokemon = species.SpawnPokemon(string.IsNullOrEmpty(name) ? null : name, health, experience);

        AnsiConsole.Status().Start("Locating Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Taming {pokemon.GetName()}...");
            Thread.Sleep(5000);

            context.Status("Pokemon caught successfully!");
            Thread.Sleep(2000);
        });

        Program.Service.AddPokemon(pokemon);
    }

    private static void ViewPokemons_ListPokemons()
    {
        var title = new FigletText("Pokémon Pocket").Color(Color.Yellow).Centered();
        var table = new Table();

        table.AddColumn("Species");
        table.AddColumn("Name");
        table.AddColumn("Health");
        table.AddColumn("Experience");
        table.AddColumn("Skill");
        table.Expand();

        var pokemons = Program.Service.GetAllPokemons();

        foreach (var pokemon in pokemons)
        {
            var speciesName = pokemon.Name;
            var pokemonName = pokemon.PetName ?? "[dim]No nickname specified.[/]";
            var health = $"{pokemon.Health}/{pokemon.MaxHealth}";
            var experience = pokemon.Experience;
            var skill = pokemon.SkillName;

            if (pokemon.Health >= pokemon.MaxHealth - 10)
                health = $"[green]{health}[/]";
            else if (pokemon.Health <= 10)
                health = $"[red]{health}[/]";
            else
                health = $"[yellow]{health}[/]";

            table.AddRow(speciesName, pokemonName, health, experience.ToString(), skill);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var prompt = new SelectionPrompt<Selection>()
            .AddChoiceGroup(
                "Actions".AsLabel(),
                "Rename".WithAction(() =>
                {
                    var subtitle = new Rule("Vet Clinic!").RuleStyle(new Style(Color.Green));

                    AnsiConsole.Clear();
                    AnsiConsole.Write(title);
                    AnsiConsole.Write(subtitle);
                    AnsiConsole.WriteLine();

                    ViewPokemons_SelectPokemon(ViewPokemons_RenamePokemon);
                }),
                "Heal".WithAction(() =>
                {
                    var subtitle = new Rule("Healing Station!").RuleStyle(new Style(Color.Green));

                    AnsiConsole.Clear();
                    AnsiConsole.Write(title);
                    AnsiConsole.Write(subtitle);
                    AnsiConsole.WriteLine();

                    ViewPokemons_SelectPokemon(ViewPokemons_HealPokemon);
                }),
                "Release".WithAction(() =>
                {
                    var subtitle = new Rule("Release Center!").RuleStyle(new Style(Color.Green));

                    AnsiConsole.Clear();
                    AnsiConsole.Write(title);
                    AnsiConsole.Write(subtitle);
                    AnsiConsole.WriteLine();

                    ViewPokemons_SelectPokemon(ViewPokemons_ReleasePokemon);
                })
            ).AddChoices(
                "Back".WithEmptyAction()
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    public static void ViewPokemons_SelectPokemon(Action<Pokemon> callback)
    {
        var pokemons = Program.Service.GetAllPokemons();
        var groups = pokemons.ToLookup(pet => pet.Name, pet => pet);

        var prompt = new SelectionPrompt<Selection>()
            .PageSize(30)
            .EnableSearch();

        foreach (var group in groups)
        {
            var choices = group.Select(pet =>
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

                return $"{name} (Health: {health}, Experience: {experience})".WithAction(() => callback(pet));
            });

            prompt.AddChoiceGroup(group.Key.AsLabel(), choices);
        }

        prompt.AddChoices("Back".WithEmptyAction());

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    public static void ViewPokemons_RenamePokemon(Pokemon pokemon)
    {
        var name = pokemon.GetName();

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

        pokemon.PetName = nameValue;
        Program.Service.SaveChanges();
    }

    public static void ViewPokemons_HealPokemon(Pokemon pokemon)
    {
        var name = pokemon.GetName();

        var healthRequired = pokemon.MaxHealth - pokemon.Health;
        var healthIncreasable = pokemon.Experience > healthRequired ? healthRequired : pokemon.Experience;
        var outputHealth = pokemon.Health + healthIncreasable;

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

        AnsiConsole.MarkupLineInterpolated($"You are about to heal [yellow]{name}[/] from [red]{pokemon.Health}[/] to [green]{outputHealth}[/]!");
        AnsiConsole.MarkupLineInterpolated($"This will consume [red]{healthIncreasable}[/] experience points.");
        AnsiConsole.WriteLine();

        var confirmation = AnsiConsole.Confirm("Do you want to proceed?");
        if (!confirmation)
            return;

        AnsiConsole.Status().Start("Healing Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Healing {pokemon.Name}...");
            Thread.Sleep(5000);

            context.Status("Pokemon healed successfully!");
            Thread.Sleep(2000);
        });

        pokemon.Health = outputHealth;
        pokemon.Experience -= healthIncreasable;
        Program.Service.SaveChanges();
    }

    public static void ViewPokemons_ReleasePokemon(Pokemon pokemon)
    {
        AnsiConsole.MarkupLineInterpolated($"You are about to release [yellow]{pokemon.GetName()}[/] into the wild!");
        AnsiConsole.MarkupLineInterpolated($"This action is [red]permanent[/] and will not be reversible.");
        AnsiConsole.WriteLine();

        var confirmation = AnsiConsole.Confirm("Do you want to proceed?");
        if (!confirmation)
            return;

        AnsiConsole.Status().Start("Releasing Pokemon...", context =>
        {
            Thread.Sleep(2000);

            context.Status($"Releasing {pokemon.Name}...");
            Thread.Sleep(5000);

            context.Status("Pokemon released successfully!");
            Thread.Sleep(2000);
        });

        Program.Service.RemovePokemon(pokemon);
    }

    public static void EvolvePokemon_ListEvolvables()
    {
        var pokemons = Program.Service.GetAllPokemons();
        var groups = pokemons.ToLookup(pet => pet.Name, pet => pet);

        var masters = Program.Service.GetAllMasters();
        var eligibles = new List<PokemonMaster>();

        var table = new Table();

        table.AddColumn("From");
        table.AddColumn("To");
        table.AddColumn("Amount");
        table.AddColumn("Evolvable?");
        table.Expand();

        foreach (var master in masters)
        {
            var from = master.Name;
            var to = master.EvolveTo;

            var fromNumber = groups.Contains(from) ? groups[from].Count() : 0;
            var requiredAmount = master.NoToEvolve;

            if (fromNumber <= 0)
                continue;

            var amount = $"{fromNumber}/{requiredAmount}";
            var evolvable = "[red]No[/]";

            if (master.CanEvolve(pokemons))
            {
                evolvable = "[green]Yes[/]";
                eligibles.Add(master);
            }

            table.AddRow(from, to, amount, evolvable);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        if (eligibles.Count > 0)
        {
            var prompt = new SelectionPrompt<Selection>()
                .AddChoiceGroup(
                    "Evolvable Masters".AsLabel(),
                    eligibles.Select(master => $"{master.Name} -> {master.EvolveTo}".WithAction(() => EvolvePokemon_SelectSacrifices(master))))
                .AddChoices(
                    "Back".WithEmptyAction()
                );

            var result = AnsiConsole.Prompt(prompt).ToAction();

            AnsiConsole.Clear();
            result.Invoke();
        }
        else
        {
            AnsiConsole.MarkupLine("[dim]No evolvable pokemons found.[/]");
            Console.ReadKey(true);
        }
    }

    public static void EvolvePokemon_SelectSacrifices(PokemonMaster master)
    {
        var title = new FigletText("Pokémon Pocket").Color(Color.Yellow).Centered();
        var subtitle = new Rule("Evolution Center!").RuleStyle(new Style(Color.Green));

        var candidates = Program.Service.GetPokemonsBySpecies(master.Name);
        var evolution = Program.Service.GetSpecies(master.EvolveTo);
        var sacrifices = new List<Pokemon>();
        var output = 0;

        var choices = candidates.Select(pokemon =>
        {
            var name = pokemon.PetName ?? pokemon.Name;
            var health = $"{pokemon.Health}/{pokemon.MaxHealth}";
            var experience = pokemon.Experience;

            if (pokemon.Health >= pokemon.MaxHealth - 10)
                health = $"[green]{health}[/]";
            else if (pokemon.Health <= 10)
                health = $"[red]{health}[/]";
            else
                health = $"[yellow]{health}[/]";


            return $"{name} (Health: {health}, Experience: {experience})".WithValue(pokemon.Id);
        });

        AnsiConsole.Write(title);
        AnsiConsole.Write(subtitle);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLineInterpolated($"You are about to evolve from [yellow]{master.Name}[/] to [yellow]{master.EvolveTo}[/]!");
        AnsiConsole.WriteLine();

        var prompt = new MultiSelectionPrompt<Selection>()
            .AddChoices(choices)
            .InstructionsText($"[dim]Please select exactly or multiples of {master.NoToEvolve} pokemon(s) from your pocket.[/]");

        while (true)
        {
            var pokemons = AnsiConsole.Prompt(prompt).ToValues<string>();

            if (pokemons.Count < master.NoToEvolve || pokemons.Count % master.NoToEvolve != 0)
                continue;

            sacrifices = pokemons.Select(id => candidates.First(pokemon => pokemon.Id == id)).ToList();
            output = pokemons.Count / master.NoToEvolve;
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

        Program.Service.RemovePokemons(sacrifices);

        var pokemon = evolution.SpawnPokemon();
        Program.Service.AddPokemon(pokemon);
    }
}