namespace PokemonPocket.Helpers;

public class SelectionAction(string name, Action callback) : ISelection
{
    public string Name => name;
    public Action Callback => callback;

    public override string ToString()
    {
        return Name;
    }
}