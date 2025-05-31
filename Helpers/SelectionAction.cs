// Dennise
// 231292A

namespace PokemonPocket.Helpers;

public class SelectionAction(string label, Action callback) : Selection(label)
{
    public Action Callback => callback;
}