using System.Numerics;
using cosmic_thing.Transforms;
using Friflo.Engine.ECS.Systems;

namespace cosmic_thing.Camera;

public class CameraDataUpdateSystem : QuerySystem<LocalToWorld, CameraData>
{
    protected override void OnUpdate()
    {
        var queryJob = Query.ForEach((localToWorldChunk, cameraDataChunk, entities) =>
        {
            for (var n = 0; n < entities.Length; n++)
            {
                ref readonly var localToWorld = ref localToWorldChunk[n];
                var worldPosition = localToWorld.Value.Translation;
                var worldRotation = Quaternion.CreateFromRotationMatrix(localToWorld.Value);
                var worldFront = Vector3.Normalize(Vector3.Transform(Program.Game.Forward, worldRotation));

                ref var cameraData = ref cameraDataChunk[n];
                cameraData.View = Matrix4x4.CreateLookAtLeftHanded(worldPosition,
                    worldPosition + worldFront, Program.Game.Up);
            }
        });
        queryJob.RunParallel();
    }
}