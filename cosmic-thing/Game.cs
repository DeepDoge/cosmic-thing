using System.Numerics;
using cosmic_thing.Camera;
using cosmic_thing.Cube;
using cosmic_thing.Player;
using cosmic_thing.Transforms;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace cosmic_thing;

public static class Game
{
    public static readonly Vector3 Forward = Vector3.UnitZ;
    public static readonly Vector3 Right = Vector3.UnitX;
    public static readonly Vector3 Up = Vector3.UnitY;
    private static readonly EntityStore World = new() { JobRunner = new ParallelJobRunner(Environment.ProcessorCount) };
    private static readonly SystemRoot SystemRoot = new(World);

    public static readonly GameWindow Window = new(GameWindowSettings.Default, new NativeWindowSettings
    {
        ClientSize = (800, 600),
        Title = "Instanced OpenGL + Camera",
        API = ContextAPI.OpenGL,
        APIVersion = new Version(4, 5),
        Profile = ContextProfile.Core
    });

    public static double DeltaTime { get; private set; }

    public static Entity MainCameraEntity { get; private set; }

    private static void Main()
    {
        Window.Load += Start;
        Window.RenderFrame += RenderFrame;
        Window.Resize += Resize;
        Window.Run();
    }

    private static void Resize(ResizeEventArgs args)
    {
        GL.Viewport(0, 0, args.Width, args.Height);
        ref var cameraData = ref MainCameraEntity.GetComponent<CameraData>();
        cameraData.AspectRatio = (float)args.Width / args.Height;
    }

    private static void Start()
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        World.CreateEntity(new Position(-1.25f, 0, 0), new ColorRgb { Value = { X = 1 } }, Tags.Get<CubeTag>());
        World.CreateEntity(new Position(0, .25f, 0), new ColorRgb { Value = { Y = 1 } }, Tags.Get<CubeTag>());
        World.CreateEntity(new Position(1.25f, .5f, 0), new ColorRgb { Value = { Z = 1 } }, Tags.Get<CubeTag>());

        MainCameraEntity = World.CreateEntity(new Position { z = -3 }, new Rotation { value = Quaternion.Identity },
            new LocalToWorld(),
            new CameraData
            {
                Fov = 90,
                NearClip = 0.1f,
                FarClip = 1000,
                AspectRatio = 1 // Updated dynamically later.
            },
            Tags.Get<FreeCamTag>());

        SystemRoot.Add(new LocalToWorldFromTrUpdateSystem());
        SystemRoot.Add(new CameraDataUpdateSystem());
        SystemRoot.Add(new FreeCamControllerSystem());
        SystemRoot.Add(new CubeRendererSystem());
    }

    private static void RenderFrame(FrameEventArgs frame)
    {
        var deltaTime = frame.Time;

        var fps = 1.0d / deltaTime;
        Console.WriteLine($"FPS: {fps:F2}");

        DeltaTime = deltaTime;

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        SystemRoot.Update(default);
        Window.SwapBuffers();
    }
}