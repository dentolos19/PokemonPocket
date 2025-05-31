// Catolos Alvaro Dennise Jay San Juan
// 231292A

using PokemonPocket.Models;

namespace PokemonPocket.Menus;

public static class BasicMenu
{
    public static void Entry()
    {
        Console.WriteLine("*****************************");
        Console.WriteLine("Welcome to Pokémon Pocket App");
        Console.WriteLine("*****************************");
        Console.WriteLine();
        Console.WriteLine("1. Add a pokemon to my pocket");
        Console.WriteLine("2. List pokemon(s) in my pocket");
        Console.WriteLine("3. Check if I can evolve my pokemons");
        Console.WriteLine("4. Evolve all pokemon(s)");
        Console.WriteLine("5. Switch to Enhanced Menu");
        Console.WriteLine();

        while (true)
        {
            Console.Write("Please only enter [1,2,3,4,5] or Q to quit: ");
            var input = Console.ReadLine()?.Trim().ToLower();

            switch (input)
            {
                case "1":
                    Console.WriteLine();
                    AddPokemon();
                    Console.WriteLine();
                    return;
                case "2":
                    Console.WriteLine();
                    ListPokemons();
                    Console.WriteLine();
                    return;
                case "3":
                    Console.WriteLine();
                    CheckEvolvable();
                    Console.WriteLine();
                    return;
                case "4":
                    Console.WriteLine();
                    EvolveAll();
                    Console.WriteLine();
                    return;
                case "5":
                    Program.ToggleMenu();
                    return;
                case "q":
                    Environment.Exit(0);
                    return;
                default:
                    Console.WriteLine("Invalid input. Please try again.");
                    break;
            }
        }
    }

    private static void AddPokemon()
    {
        string? name = null;
        int? health = null;
        int? experience = null;

        while (string.IsNullOrEmpty(name))
        {
            Console.Write("Enter Pokemon's Name: ");
            var nameCandidate = Console.ReadLine();

            if (string.IsNullOrEmpty(nameCandidate))
            {
                Console.WriteLine("Name cannot be empty. Please try again.");
                continue;
            }

            if (!Program.Service.CheckPokemonExists(nameCandidate))
            {
                Console.WriteLine("Pokemon does not exist. Please try again.");
                continue;
            }

            name = nameCandidate;
        }

        while (health == null)
        {
            Console.Write("Enter Pokemon's Health: ");
            var healthCandidate = Console.ReadLine();

            if (!int.TryParse(healthCandidate, out var healthValue))
            {
                Console.WriteLine("Health must be a number. Please try again.");
                continue;
            }

            if (healthValue <= 0)
            {
                Console.WriteLine("Health must be greater than 0. Please try again.");
                continue;
            }

            health = healthValue;
        }

        while (experience == null)
        {
            Console.Write("Enter Pokemon's Experience: ");
            var experienceCandidate = Console.ReadLine();

            if (!int.TryParse(experienceCandidate, out var experienceValue))
            {
                Console.WriteLine("Experience must be a number. Please try again.");
                continue;
            }

            if (experienceValue < 0)
            {
                Console.WriteLine("Experience must be greater than or equal to 0. Please try again.");
                continue;
            }

            experience = experienceValue;
        }

        var pokemon = Program.Service.GetPokemon(name);
        var pet = new Pokemon();

        pet.EvolveTo(pokemon);
        pet.Health = health.Value;
        pet.Experience = experience.Value;

        Program.Service.AddPet(pet);
    }

    private static void ListPokemons()
    {
        var pets = Program.Service.GetAllPets().OrderByDescending(pokemon => pokemon.Experience);

        if (pets.Any())
        {
            foreach (var pet in pets)
            {
                Console.WriteLine($"Name: {pet.Name}");
                Console.WriteLine($"Health: {pet.MaxHealth}");
                Console.WriteLine($"Experience: {pet.Experience}");
                Console.WriteLine($"Skill: {pet.SkillName}");
                Console.WriteLine("-----------------------");
            }
        }
        else
        {
            Console.WriteLine("No pokemons found in your pocket.");
        }

        Console.ReadKey();
    }

    private static void CheckEvolvable()
    {
        var pets = Program.Service.GetAllPets();
        var masters = Program.Service.GetAllMasters();
        var evolvableCount = 0;

        foreach (var master in masters)
        {
            if (!master.CanEvolve(pets))
                continue;

            var petCount = pets.Where(pet => pet.Name == master.Name).Count();
            var evolutionMultiple = petCount >= master.NoToEvolve
                ? (int)Math.Floor((double)(petCount / master.NoToEvolve))
                : 0;

            Console.WriteLine(
                $"{master.NoToEvolve * evolutionMultiple} {master.Name} --> {evolutionMultiple} {master.EvolveTo}");

            evolvableCount++;
        }

        if (evolvableCount == 0)
        {
            Console.WriteLine("No pokemons are eligible for evolution.");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine($"Total evolvable pokemons: {evolvableCount}");
        }

        Console.ReadKey();
    }

    private static void EvolveAll()
    {
        var masters = Program.Service.GetAllMasters();
        foreach (var master in masters)
        {
            // Refresh pets list for each master to get current state
            var pets = Program.Service.GetAllPets();

            if (!master.CanEvolve(pets))
                continue;

            // Get all pets of the same species that need to be evolved
            var petsToEvolve = pets
                .Where(pokemon => pokemon.Name == master.Name)
                .Take(master.NoToEvolve)
                .ToList();

            if (petsToEvolve.Count < master.NoToEvolve)
                continue;

            // Remove all pets in a single batch operation
            Program.Service.RemovePets(petsToEvolve);

            var evolvedPokemon = Program.Service.GetPokemon(master.EvolveTo);
            var evolvedPet = evolvedPokemon.SpawnPet();

            Program.Service.AddPet(evolvedPet);
        }

        Console.WriteLine("Evolved all eligible pokemons.");

        Console.ReadKey();
    }
}