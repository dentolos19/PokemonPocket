using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PokemonPocket.Models;

public class Pokemon
{
    [Key] public string Id { get; init; } = Guid.NewGuid().ToString();

    // Properties
    public virtual string Name { get; private set; }
    public virtual int MaxHealth { get; private set; }
    public virtual int DamageMultiplier { get; private set; } = 1;

    // Skill
    public virtual string SkillName { get; private set; }
    public virtual int SkillDamage { get; private set; }

    // User
    public string? PetName { get; set; }
    public int Health { get; set; }
    public int Experience { get; set; }

    public void CalculateDamage(int damage)
    {
        Health -= damage * DamageMultiplier;
    }

    public void EvolveTo(Pokemon pokemon)
    {
        Name = pokemon.Name;
        MaxHealth = pokemon.MaxHealth;
        SkillName = pokemon.SkillName;
        SkillDamage = pokemon.SkillDamage;
        DamageMultiplier = pokemon.DamageMultiplier;
    }

    public Pokemon SpawnPet(string? name, int health, int experience)
    {
        var pet = new Pokemon();
        pet.EvolveTo(this);

        pet.PetName = name;
        pet.Health = health;
        pet.Experience = experience;

        return pet;
    }
}