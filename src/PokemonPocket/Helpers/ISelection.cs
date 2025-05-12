namespace PokemonPocket.Helpers;

public interface ISelection
{
    public string Name { get; }
    public string ToString() => Name;
}