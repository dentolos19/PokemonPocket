// Catolos Alvaro Dennise Jay San Juan
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
        var finalDamage = damage * DamageMultiplier;
        Health -= finalDamage;

        return finalDamage;
    }

    public int DealDamage()
    {
        return SkillDamage;
    }

    public void EvolveTo(Pokemon pokemon)
    {
        Name = pokemon.Name;
        MaxHealth = pokemon.MaxHealth;
        SkillName = pokemon.SkillName;
        SkillDamage = pokemon.SkillDamage;
        DamageMultiplier = pokemon.DamageMultiplier;
    }

    public string GetName()
    {
        return string.IsNullOrEmpty(PetName) ? Name : PetName;
    }

    public Pokemon SpawnPokemon(string? name = null, int health = 100, int experience = 0)
    {
        var pet = new Pokemon();
        pet.EvolveTo(this);

        pet.PetName = name;
        pet.Health = health;
        pet.Experience = experience;

        return pet;
    }
}