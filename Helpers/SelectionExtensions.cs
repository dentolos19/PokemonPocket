namespace PokemonPocket.Helpers;

public static class SelectionExtensions
{
    public static SelectionAction WithAction(this string choice, Action callback)
    {
        return new SelectionAction(choice, callback);
    }

    public static SelectionAction WithEmptyAction(this string choice)
    {
        return new SelectionAction(choice, () => { });
    }

    public static SelectionValue<T> WithValue<T>(this string choice, T value)
    {
        return new SelectionValue<T>(choice, value);
    }

    public static SelectionValue<T> WithEmptyValue<T>(this string choice)
    {
        return new SelectionValue<T>(choice, default);
    }
}