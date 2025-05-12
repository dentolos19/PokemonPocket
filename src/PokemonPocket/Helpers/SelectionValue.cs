namespace PokemonPocket.Helpers;

public class SelectionValue<T>(string name, T value) : ISelection
{
    public string Name => name;
    public T Value => value;

    public override string ToString()
    {
        return Name;
    }
}