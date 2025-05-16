using PokemonPocket.Models;

namespace PokemonPocket.Menus;

public static class BasicMenu
{
    public static void Start()
    {
        Console.WriteLine("*****************************");
        Console.WriteLine("Welcome to Pokemon Pocket App");
        Console.WriteLine("*****************************");
        Console.WriteLine();
        Console.WriteLine("1. Add a pokemon to my pocket");
        Console.WriteLine("2. List pokemon(s) in my pocket");
        Console.WriteLine("3. Check if I can evolve my pokemons");
        Console.WriteLine("4. Evolve all pokemon(s)");
        Console.WriteLine();

        while (true)
        {
            Console.Write("Please only enter [1,2,3,4] or Q to quit: ");
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

        if (!pets.Any())
        {
            Console.WriteLine("No pokemons found in your pocket.");
            return;
        }

        foreach (var pet in pets)
        {
            Console.WriteLine($"Name: {pet.Name}");
            Console.WriteLine($"Health: {pet.Health}");
            Console.WriteLine($"Experience: {pet.Experience}");
            Console.WriteLine($"Skill: {pet.SkillName}");
            Console.WriteLine("-----------------------");
        }
    }

    private static void CheckEvolvable()
    {
        var pets = Program.Service.GetAllPets();
        var masters = Program.Service.GetAllMasters();

        foreach (var master in masters)
        {
            var amount = master.GetEvolvableAmount(pets);
            if (amount <= 0)
                continue;

            Console.WriteLine($"{master.NoToEvolve * amount} {master.Name} --> {amount} {master.EvolveTo}");
        }
    }

    private static void EvolveAll()
    {
        var pets = Program.Service.GetAllPets();
        var masters = Program.Service.GetAllMasters();

        foreach (var master in masters)
        {
            var amount = master.GetEvolvableAmount(pets);
            if (amount <= 0)
                continue;

            for (var index = 0; index < amount; index++)
            {
                var pet = pets.FirstOrDefault(pokemon => pokemon.Name == master.Name);
                Program.Service.RemovePet(pet);
            }

            var evolvedPokemon = Program.Service.GetPokemon(master.EvolveTo);
            var evolvedPet = new Pokemon();
            evolvedPet.EvolveTo(evolvedPokemon);

            evolvedPet.Health = 100;
            evolvedPet.Experience = 0;

            Program.Service.AddPet(evolvedPet);
        }

        Console.WriteLine("Evolved all eligible pokemons.");
    }
}