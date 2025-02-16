using System.Numerics;
using cosmic_thing.Camera;
using cosmic_thing.Player;
using cosmic_thing.Transforms;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

namespace cosmic_thing;

public class Game : IDisposable
{
    public readonly Vector3 Forward = Vector3.UnitZ;
    public readonly Vector3 Right = Vector3.UnitX;
    public readonly Vector3 Up = Vector3.UnitY;

    public double DeltaTime;

    public Entity MainCameraEntity;
    public required ParallelJobRunner ParallelJobRunner;
    public required SystemRoot SystemRoot;
    public required EntityStore World;

    public void Dispose()
    {
        ParallelJobRunner.Dispose();
        GC.SuppressFinalize(this);
    }

    public static Game Default()
    {
        var parallelJobRunner = new ParallelJobRunner(Environment.ProcessorCount);
        var world = new EntityStore { JobRunner = parallelJobRunner };
        var systemRoot = new SystemRoot(world);

        return new Game
        {
            World = world,
            SystemRoot = systemRoot,
            ParallelJobRunner = parallelJobRunner
        };
    }

    public void Start()
    {
        World.CreateEntity(new Position(-1.25f, 0, 0), new ColorRgb { Value = { X = 1 } }, Tags.Get<CubeTag>());
        World.CreateEntity(new Position(0, .25f, 0), new ColorRgb { Value = { Y = 1 } }, Tags.Get<CubeTag>());
        World.CreateEntity(new Position(1.25f, .5f, 0), new ColorRgb { Value = { Z = 1 } }, Tags.Get<CubeTag>());

        MainCameraEntity = World.CreateEntity(new Position { z = -3 }, new Rotation { value = Quaternion.Identity },
            new LocalToWorld(),
            new CameraData(),
            Tags.Get<PlayerTag>());

        SystemRoot.Add(new LocalToWorldFromTrUpdateSystem());
    }

    public void Update(double deltaTime)
    {
        var fps = 1.0d / deltaTime;
        Console.WriteLine($"FPS: {fps:F2}");

        DeltaTime = deltaTime;
        SystemRoot.Update(default);
    }
}