using System.Numerics;
using Friflo.Engine.ECS;

namespace cosmic_thing.Camera;

public struct CameraData : IComponent
{
    public Matrix4x4 Projection;
    public Matrix4x4 View;
}