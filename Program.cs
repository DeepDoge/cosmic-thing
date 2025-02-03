using Friflo.Engine.ECS;
using OpenTK.Windowing.Desktop;

namespace cosmic_thing;

public static class Program
{
    public static void Main()
    {
        using var window = new OpenGlWindow();
        window.Run();
    }
}