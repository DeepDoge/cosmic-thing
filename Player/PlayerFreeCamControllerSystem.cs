using System.Numerics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace cosmic_thing.Player;

public class PlayerFreeCamControllerSystem : QuerySystem<Position, Rotation>
{
    private float pitchAngle; // store pitch separately

    public PlayerFreeCamControllerSystem()
    {
        Filter.AllTags(Tags.Get<PlayerTag>());
    }

    protected override void OnUpdate()
    {
        const float moveSpeed = 5f;
        const float sensitivity = 0.1f;
        var deltaTime = (float)Program.Game.DeltaTime;
        var velocity = moveSpeed * deltaTime;

        var keyboardState = Program.Window.KeyboardState;
        var mouseState = Program.Window.MouseState;

        Query.ForEachEntity((ref Position position, ref Rotation rotation, Entity entity) =>
        {
            // get local movement directions based on current rotation
            var forward = Vector3.Transform(Program.Game.Forward, rotation.value);
            var right = Vector3.Transform(Program.Game.Right, rotation.value);

            // movement (only modifies position)
            if (keyboardState.IsKeyDown(Keys.W))
                position.value += forward * velocity;
            if (keyboardState.IsKeyDown(Keys.S))
                position.value -= forward * velocity;
            if (keyboardState.IsKeyDown(Keys.A))
                position.value -= right * velocity;
            if (keyboardState.IsKeyDown(Keys.D))
                position.value += right * velocity;

            // only rotate if left mouse button is held
            if (!mouseState.IsButtonDown(MouseButton.Left)) return;
            var mouseDelta = mouseState.Delta;
            var yaw = float.DegreesToRadians(mouseDelta.X * sensitivity);
            var pitch = float.DegreesToRadians(mouseDelta.Y * sensitivity);

            // yaw: rotate around world up (Vector3.UnitY)
            var yawRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
            rotation.value = Quaternion.Normalize(yawRotation * rotation.value);

            // update pitch angle and clamp to avoid flipping
            pitchAngle = Math.Clamp(pitchAngle + pitch, -MathF.PI / 2 + 0.1f, MathF.PI / 2 - 0.1f);

            // get new right vector after yaw rotation
            right = Vector3.Transform(Vector3.UnitX, rotation.value);

            // pitch: rotate around new local right axis
            var pitchRotation = Quaternion.CreateFromAxisAngle(right, pitch);
            rotation.value = Quaternion.Normalize(pitchRotation * rotation.value);
        });
    }
}