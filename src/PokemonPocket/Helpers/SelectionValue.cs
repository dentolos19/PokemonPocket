namespace PokemonPocket.Helpers;

public class SelectionValue<T>(string label, T? value) : Selection(label)
{
    public T? Value => value;
}