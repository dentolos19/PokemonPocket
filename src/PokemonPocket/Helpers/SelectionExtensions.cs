namespace PokemonPocket.Helpers;

public static class SelectionExtensions
{
    public static Selection AsLabel(this string label)
    {
        return new Selection(label);
    }

    public static Selection WithAction(this string label, Action callback)
    {
        return new SelectionAction(label, callback);
    }

    public static Selection WithEmptyAction(this string label)
    {
        return new SelectionAction(label, () => { });
    }

    public static Selection WithValue<T>(this string label, T? value)
    {
        return new SelectionValue<T?>(label, value);
    }

    public static Selection WithDefaultValue<T>(this string label)
    {
        return new SelectionValue<T?>(label, default);
    }

    public static SelectionAction ToAction(this Selection selection)
    {
        if (selection is SelectionAction action)
            return action;
        throw new Exception("Invalid selection type.");
    }

    public static T ToValue<T>(this Selection selection)
    {
        if (selection is SelectionValue<T> selectionValue)
            return selectionValue.Value!;
        throw new Exception("Invalid selection type.");
    }

    public static IList<T> ToValues<T>(this IList<Selection> selections)
    {
        var values = new List<T>();

        foreach (var selection in selections)
        {
            if (selection is not SelectionValue<T> selectionValue)
                continue;
            values.Add(selectionValue.Value!);
        }

        return values;
    }
}