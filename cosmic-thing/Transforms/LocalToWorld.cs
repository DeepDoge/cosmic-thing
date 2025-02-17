using System.Numerics;
using Friflo.Engine.ECS;

namespace cosmic_thing.Transforms;

public struct LocalToWorld : IComponent
{
    public Matrix4x4 Value;
}