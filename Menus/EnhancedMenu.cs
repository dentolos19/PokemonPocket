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
                "Add A Pokémon".WithAction(AddPokemon_SelectSpecies),
                "View My Pokémons".WithAction(ViewPokemons_ListPokemons),
                "Evolve My Pokémons".WithAction(EvolvePokemon_ListEvolvables),
                "View Evolution Chains".WithAction(EvolutionChains)
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
        var wildPokemon = availableSpecies.OrderBy(_ => Guid.NewGuid()).First().SpawnPokemon();

        // Randomly set the health of the wild Pokémon between 60% and 100% of its max health
        var healthPercentage = Random.Shared.Next(60, 101) / 100.0;
        wildPokemon.Health = (int)(wildPokemon.MaxHealth * healthPercentage);

        AnsiConsole.MarkupLineInterpolated($"A wild [yellow]{wildPokemon.Name}[/] has been spotted!");
        AnsiConsole.WriteLine();

        var table = new Table();

        table.AddColumn(new TableColumn("[italic]Property[/]"));
        table.AddColumn(new TableColumn("[italic]Value[/]"));

        table.AddRow("Health", $"{wildPokemon.Health}/{wildPokemon.MaxHealth}");
        table.AddRow("Skill Name", wildPokemon.SkillName);
        table.AddRow("Skill Damage", wildPokemon.SkillDamage.ToString());

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var prompt = new SelectionPrompt<Selection>()
            .AddChoices(
                "Target Pokemon".WithAction(() => CatchPokemon_Draft(wildPokemon)),
                "Move On".WithAction(CatchPokemon_Wilding),
                "Back".WithEmptyAction()
            );

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    private static void CatchPokemon_Draft(Pokemon wild)
    {
        var pokemons = Program.Service.GetAllPokemons();

        if (pokemons.Length > 0)
        {
            var groups = pokemons.ToLookup(pokemon => pokemon.Name, pokemon => pokemon);

            var prompt = new SelectionPrompt<Selection>()
                .PageSize(30)
                .EnableSearch()
                .SearchPlaceholderText("[dim]Type to search for a Pokémon.[/]");

            foreach (var group in groups)
            {
                var groupPokemons = group.Where(pokemon => pokemon.Health > 0).ToArray();
                if (groupPokemons.Length <= 0)
                    continue;

                var choices = groupPokemons.Select(draft =>
                {
                    var name = draft.GetName();
                    var health = $"{draft.Health}/{draft.MaxHealth}";
                    var experience = draft.Experience;

                    if (draft.Health >= draft.MaxHealth - 10)
                        health = $"[green]{health}[/]";
                    else if (draft.Health <= 10)
                        health = $"[red]{health}[/]";
                    else
                        health = $"[yellow]{health}[/]";

                    return $"{name} (Health: {health}, Experience: {experience})".WithAction(() => CatchPokemon_Catch(wild, draft));
                });

                prompt.AddChoiceGroup(group.Key.AsLabel(), choices);
            }

            prompt.AddChoices("Back".WithEmptyAction());

            var result = AnsiConsole.Prompt(prompt).ToAction();
            result.Invoke();
        }
        else
        {
            AnsiConsole.MarkupLine("[dim]No pokemons found in your pocket to catch with.[/]");
            Console.ReadKey();
        }
    }

    private static void CatchPokemon_Catch(Pokemon wild, Pokemon draft)
    {
        AnsiConsole.Clear();

        var battleTitle = new FigletText("Pokémon Pocket").Color(Color.Yellow).Centered();
        var battleSubtitle = new Rule("Battle Confirmation!").RuleStyle(new Style(Color.Orange1));

        AnsiConsole.Write(battleTitle);
        AnsiConsole.Write(battleSubtitle);
        AnsiConsole.WriteLine();

        var comparisonTable = new Table();

        comparisonTable.AddColumn(new TableColumn("[bold]Property[/]"));
        comparisonTable.AddColumn(new TableColumn($"[green]{draft.GetName()}[/] (Your Pokémon)").Centered());
        comparisonTable.AddColumn(new TableColumn($"[red]{wild.Name}[/] (Wild Pokémon)").Centered());

        comparisonTable.AddRow("Health", $"{draft.Health}/{draft.MaxHealth}", $"{wild.Health}/{wild.MaxHealth}");
        comparisonTable.AddRow("Experience", draft.Experience.ToString(), "Unknown");
        comparisonTable.AddRow("Skill Name", draft.SkillName, wild.SkillName);
        comparisonTable.AddRow("Skill Damage", draft.SkillDamage.ToString(), wild.SkillDamage.ToString());

        AnsiConsole.Write(comparisonTable);
        AnsiConsole.WriteLine();

        var confirmation = AnsiConsole.Confirm($"Start battle between [green]{draft.GetName()}[/] and [red]{wild.Name}[/]?");
        if (!confirmation)
            return;

        AnsiConsole.Clear();

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

            var draftHealthBar = new BarChartItem(draftName, draftHealth, Color.Green);
            var wildHealthBar = new BarChartItem(wildName, wildHealth, Color.Red);

            // Render health bars
            var chart = new BarChart()
                .Width(50)
                .WithMaxValue(Math.Max(draft.MaxHealth, wild.MaxHealth))
                .AddItem(draftHealthBar)
                .AddItem(wildHealthBar);

            AnsiConsole.Write(title);
            AnsiConsole.Write(subtitle);
            AnsiConsole.WriteLine();

            AnsiConsole.Write(chart);
            AnsiConsole.WriteLine();

            // AnsiConsole.MarkupLineInterpolated($"[green]{draftName}[/] HP: {draftHealth}");
            // AnsiConsole.MarkupLineInterpolated($"[red]{wildName}[/] HP: {wildHealth}");
            // AnsiConsole.WriteLine();

            // If both are defeated...
            if (draftHealth <= 0 && wildHealth <= 0)
            {
                var gainedExperience = Random.Shared.Next(10, 30);

                AnsiConsole.MarkupLineInterpolated($"Both have fainted! You've failed to catch [red]{wildName}[/]!");
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

                AnsiConsole.MarkupLineInterpolated($"[green]{draftName}[/] has fainted! You failed to catch [red]{wildName}[/]!");
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

                        AnsiConsole.MarkupLineInterpolated($"[green]{draftName}[/] used [cyan]{draft.SkillName}[/] to deal [yellow]{draftDamage}[/] damage to [red]{wildName}[/]!");
                        Thread.Sleep(1000);
                        wildHealth -= draftDamage;

                        AnsiConsole.MarkupLineInterpolated($"[red]{wildName}[/] used [cyan]{wild.SkillName}[/] to deal [yellow]{wildDamage}[/] damage to [green]{draftName}[/]!");
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

    private static void AddPokemon_SelectSpecies()
    {
        var pokemons = Program.Service.GetAllSpecies();
        var choices = pokemons.Select(pokemon => pokemon.Name.WithAction(() => AddPokemon_SetDetails(pokemon.Name)));

        var prompt = new SelectionPrompt<Selection>()
            .PageSize(100)
            .EnableSearch()
            .SearchPlaceholderText("[dim]Type to search for a Species.[/]")
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

        var table = new Table();

        table.AddColumn(new TableColumn("[italic]Property[/]"));
        table.AddColumn(new TableColumn("[italic]Value[/]"));

        table.AddRow("Max Health", species.MaxHealth.ToString());
        table.AddRow("Skill Name", species.SkillName);
        table.AddRow("Skill Damage", species.SkillDamage.ToString());

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        var confirmation = AnsiConsole.Confirm("Do you want to proceed?");
        if (!confirmation)
            return;

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

        table.AddColumn(new TableColumn("[italic]Species[/]").Centered());
        table.AddColumn(new TableColumn("[italic]Name[/]").Centered());
        table.AddColumn(new TableColumn("[italic]Health[/]").Centered());
        table.AddColumn(new TableColumn("[italic]Experience[/]").Centered());
        table.AddColumn(new TableColumn("[italic]Skill[/]").Centered());
        table.Expand();

        var pokemons = Program.Service.GetAllPokemons();

        if (pokemons.Length > 0)
        {
            foreach (var pokemon in pokemons)
            {
                var speciesName = $"[cyan]{pokemon.Name}[/]";
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
        else
        {
            AnsiConsole.MarkupLine("[dim]No pokemons found in your pocket.[/]");
            Console.ReadKey();
        }

    }

    private static void ViewPokemons_SelectPokemon(Action<Pokemon> callback)
    {
        var pokemons = Program.Service.GetAllPokemons();
        var groups = pokemons.ToLookup(pokemon => pokemon.Name, pokemon => pokemon);

        var prompt = new SelectionPrompt<Selection>()
            .PageSize(30)
            .EnableSearch()
            .SearchPlaceholderText("[dim]Type to search for a Pokémon.[/]");

        foreach (var group in groups)
        {
            var choices = group.Select(pokemon =>
            {
                var name = pokemon.GetName();
                var health = $"{pokemon.Health}/{pokemon.MaxHealth}";
                var experience = pokemon.Experience;

                if (pokemon.Health >= pokemon.MaxHealth - 10)
                    health = $"[green]{health}[/]";
                else if (pokemon.Health <= 10)
                    health = $"[red]{health}[/]";
                else
                    health = $"[yellow]{health}[/]";

                return $"{name} (Health: {health}, Experience: {experience})".WithAction(() => callback(pokemon));
            });

            prompt.AddChoiceGroup(group.Key.AsLabel(), choices);
        }

        prompt.AddChoices("Back".WithEmptyAction());

        var result = AnsiConsole.Prompt(prompt).ToAction();
        result.Invoke();
    }

    private static void ViewPokemons_RenamePokemon(Pokemon pokemon)
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

    private static void ViewPokemons_HealPokemon(Pokemon pokemon)
    {
        var name = pokemon.GetName();

        var healthRequired = pokemon.MaxHealth - pokemon.Health;
        var healthIncreasable = pokemon.Experience > healthRequired ? healthRequired : pokemon.Experience;
        var outputHealth = pokemon.Health + healthIncreasable;

        if (healthRequired <= 0)
        {
            AnsiConsole.MarkupLineInterpolated($"[yellow]{name}[/] is already at full health!");
            Console.ReadKey();
            return;
        }

        if (healthIncreasable <= 0)
        {
            AnsiConsole.MarkupLineInterpolated($"[yellow]{name}[/] does not have enough experience to heal!");
            Console.ReadKey();
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

    private static void ViewPokemons_ReleasePokemon(Pokemon pokemon)
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

    private static void EvolvePokemon_ListEvolvables()
    {
        var pokemons = Program.Service.GetAllPokemons();
        var groups = pokemons.ToLookup(pokemon => pokemon.Name, pokemon => pokemon);

        var masters = Program.Service.GetAllMasters();
        var eligibles = new List<PokemonMaster>();

        var table = new Table();

        table.AddColumn(new TableColumn("[italic]From Species[/]").Centered());
        table.AddColumn(new TableColumn("[italic]To Species[/]").Centered());
        table.AddColumn(new TableColumn("[italic]Required Amount[/]").Centered());
        table.AddColumn(new TableColumn("[italic]Evolvable?[/]").Centered());
        table.Expand();

        foreach (var master in masters)
        {
            var fromSpecies = $"[cyan]{master.Name}[/]";
            var toSpecies = $"[yellow]{master.EvolveTo}[/]";

            var currentCount = groups.Contains(master.Name) ? groups[master.Name].Count() : 0;
            var requiredCount = master.NoToEvolve;

            if (currentCount <= 0)
                continue;

            var requiredAmount = $"{currentCount}/{requiredCount}";
            var evolvable = "[red]No[/]";

            if (master.CanEvolve(pokemons))
            {
                evolvable = "[green]Yes[/]";
                eligibles.Add(master);
            }

            table.AddRow(fromSpecies, toSpecies, requiredAmount, evolvable);
        }

        if (eligibles.Count > 0)
        {
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

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
            Console.ReadKey();
        }
    }

    private static void EvolvePokemon_SelectSacrifices(PokemonMaster master)
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

    private static void EvolutionChains()
    {
        var title = new FigletText("Pokémon Pocket").Color(Color.Yellow).Centered();
        var subtitle = new Rule("Evolution Chains!").RuleStyle(new Style(Color.Green));

        AnsiConsole.Clear();
        AnsiConsole.Write(title);
        AnsiConsole.Write(subtitle);
        AnsiConsole.WriteLine();

        var masters = Program.Service.GetAllMasters();
        var chains = new Dictionary<string, List<PokemonMaster>>();
        var processed = new HashSet<string>();

        foreach (var master in masters)
        {
            if (processed.Contains(master.Name))
                continue;

            // Find the start of this evolution chain
            var chainStart = Utilities.FindChainStart(master.Name, masters);
            if (processed.Contains(chainStart))
                continue;

            // Build the complete chain from start
            var chain = new List<PokemonMaster>();
            var currentSpecies = chainStart;

            while (true)
            {
                var evolutionRule = masters.FirstOrDefault(master => master.Name == currentSpecies);
                if (evolutionRule == null)
                    break;

                chain.Add(evolutionRule);
                processed.Add(currentSpecies);
                currentSpecies = evolutionRule.EvolveTo;

                // Check if this evolution leads to another evolution
                if (!masters.Any(master => master.Name == currentSpecies))
                    break;
            }

            if (chain.Count > 0)
            {
                chains[chainStart] = chain;
            }
        }

        var table = new Table();

        table.AddColumn("[italic]Evolution Chain[/]");
        table.AddColumn("[italic]Requirements[/]");
        table.Expand();

        foreach (var (chainStart, chainMasters) in chains)
        {
            var chainDisplay = new List<string>();
            var requirementDisplay = new List<string>();

            // Add the first species (base form)
            chainDisplay.Add($"[cyan]{chainStart}[/]");

            // Add each evolution step
            foreach (var master in chainMasters)
            {
                chainDisplay.Add($"[yellow]{master.EvolveTo}[/]");
                requirementDisplay.Add($"[dim]{master.NoToEvolve} {master.Name}[/]");
            }

            var chainText = string.Join(" -> ", chainDisplay);
            var requirementText = string.Join(" -> ", requirementDisplay);

            table.AddRow(chainText, requirementText);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[dim]Press any key to return to menu...[/]");
        Console.ReadKey();
    }
}