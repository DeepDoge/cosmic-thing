namespace cosmic_thing;

public static class Program
{
    public static readonly Game Game = Game.Default();
    public static readonly OpenGlWindow Window = new() { Game = Game };

    public static void Main()
    {
        Window.Run();
        Game.Dispose();
        Window.Dispose();
    }
}