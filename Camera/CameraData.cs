using System.Numerics;
using Friflo.Engine.ECS;

namespace cosmic_thing.Camera;

public struct CameraData : IComponent
{
    // TODO: Maybe separate this into multiple components?
    public Matrix4x4 Projection;
    public Matrix4x4 View;

    // TODO: Maybe create functions here to update values below.
    //  That way we can update projection matrix only during change.
    //  But it's fine for now. I don't think its a BIG issue that its being updated every frame atm.
    public required float Fov;
    public required float NearClip;
    public required float FarClip;
    public required float AspectRatio;
}