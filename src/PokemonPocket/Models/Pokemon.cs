using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PokemonPocket.Models;

public class Pokemon
{
    [Key] public string Id { get; set; } = Guid.NewGuid().ToString();

    // Basic
    public virtual string Name { get; set; }
    public virtual int Health { get; set; }
    public virtual int Experience { get; set; }

    // Properties
    public virtual int DamageMultiplier { get; set; } = 1;

    // Skill
    public virtual string SkillName { get; set; }
    public virtual int SkillDamage { get; set; }

    public void CalculateDamage(int damage)
    {
        Health -= damage * 1;
    }

    public void EvolveTo(Pokemon pokemon)
    {
        Name = pokemon.Name;
        Health = pokemon.Health;
        Experience = pokemon.Experience;
        SkillName = pokemon.SkillName;
        SkillDamage = pokemon.SkillDamage;
        DamageMultiplier = pokemon.DamageMultiplier;
    }
}