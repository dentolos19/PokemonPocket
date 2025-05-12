namespace PokemonPocket;

internal static class Program
{
    public static PokemonService Service = new();

    public static void Main()
    {
        while (true)
        {
            Menus.MainMenu();
        }
    }
}