using System.Drawing;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace cosmic_thing;

public class OpenGlWindow() : GameWindow(GameWindowSettings.Default, new NativeWindowSettings
{
    ClientSize = new Vector2i(800, 600),
    Title = "Instanced OpenGL + Camera",
    API = ContextAPI.OpenGL,
    APIVersion = new Version(4, 5),
    Profile = ContextProfile.Core
})
{
    private EntityStore _world = new();
    private SystemRoot _systemRoot = null!;
    private Camera _camera = null!;

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        _world = new EntityStore();
        _camera = new Camera(new Vector3(0, 0, 3));
        
        _world.CreateEntity(new Position(), new CubeColor { Value = { X = 1} }, Tags.Get<CubeTag>());
        _world.CreateEntity(new Position(1.25f, 0, 0), new CubeColor { Value = { Y = 1 } }, Tags.Get<CubeTag>());
        _world.CreateEntity(new Position(-1.25f, 0, 0), new CubeColor { Value = { Z = 1 } }, Tags.Get<CubeTag>());

        _systemRoot = new SystemRoot(_world)
        {
            new CubeRenderer
            {
                MainCamera = _camera
            }
        };
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // 🔴 Pass input to camera
        _camera.Update(KeyboardState, MouseState, MousePosition, args.Time);

        // 🔴 Call ECS update (Rendering)
        _systemRoot.Update(default);

        SwapBuffers();
    }
}