using System.Numerics;
using cosmic_thing.Camera;
using cosmic_thing.Cube;
using cosmic_thing.Player;
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
    public required Game Game;

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        Game.Start();
        Game.SystemRoot.Add(new CameraDataUpdateSystem());
        Game.SystemRoot.Add(new PlayerFreeCamControllerSystem());
        Game.SystemRoot.Add(new CubeRenderer());
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);

        var aspectRatio = (float)e.Width / e.Height;
        ref var cameraData = ref Game.MainCameraEntity.GetComponent<CameraData>();
        cameraData.Projection =
            Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(float.DegreesToRadians(70f), aspectRatio, 0.1f, 100f);
        ;
    }

    protected override void OnRenderFrame(FrameEventArgs frame)
    {
        base.OnRenderFrame(frame);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        Game.Update(frame.Time);
        SwapBuffers();
    }
}