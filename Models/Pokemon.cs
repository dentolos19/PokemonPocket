// Dennise
// 231292A

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PokemonPocket.Models;

public class Pokemon
{
    [Key] public string Id { get; init; } = Guid.NewGuid().ToString();

    // Species' Properties
    public virtual string Name { get; private set; }
    public virtual int MaxHealth { get; private set; }
    public virtual int DamageMultiplier { get; private set; } = 1;

    // Species' Skill
    public virtual string SkillName { get; private set; }
    public virtual int SkillDamage { get; private set; }

    // Pet Properties
    public string? PetName { get; set; }
    public int Health { get; set; }
    public int Experience { get; set; }

    public int CalculateDamage(int damage)
    {
        int finalDamage;

        if (Program.Mode == ProgramMode.Basic)
        {
            // As per assignment requirement
            finalDamage = damage * DamageMultiplier;
        }
        else
        {
            // Apply a random multiplier between 0.5 and 1.5
            var randomMultiplier = new Random().NextDouble() * (1.5 - 0.5) + 0.5;
            finalDamage = (int)(damage * DamageMultiplier * randomMultiplier);
        }

        Health = Math.Max(0, Health - finalDamage);
        return finalDamage;
    }

    public int DealDamage()
    {
        return SkillDamage;
    }

    public string GetName()
    {
        return string.IsNullOrEmpty(PetName) ? Name : PetName;
    }

    public Pokemon SpawnPokemon(string? name = null, int health = 100, int experience = 0)
    {
        var pet = new Pokemon();

        pet.Name = Name;
        pet.MaxHealth = MaxHealth;
        pet.DamageMultiplier = DamageMultiplier;
        pet.SkillName = SkillName;
        pet.SkillDamage = SkillDamage;

        pet.PetName = name;
        pet.Health = health;
        pet.Experience = experience;

        return pet;
    }
}