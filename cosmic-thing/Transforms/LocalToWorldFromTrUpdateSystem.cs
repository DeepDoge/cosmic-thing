using System.Numerics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

namespace cosmic_thing.Transforms;

public class LocalToWorldFromTrUpdateSystem : QuerySystem<Position, Rotation, LocalToWorld>
{
    protected override void OnUpdate()
    {
        Query.ForEach((positionChunk, rotationChunk, localToWorldChunk, entities) =>
        {
            for (var i = 0; i < entities.Length; ++i)
            {
                ref readonly var position = ref positionChunk[i];
                ref readonly var rotation = ref rotationChunk[i];
                ref var localToWorld = ref localToWorldChunk[i];

                // TODO: Later calculate real world matrix when you have parent and stuff.
                localToWorld.Value = Matrix4x4.CreateScale(1) *
                                     Matrix4x4.CreateFromQuaternion(rotation.value) *
                                     Matrix4x4.CreateTranslation(position.value);
            }
        }).RunParallel();
    }
}