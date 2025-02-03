using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace cosmic_thing;

public class Camera(Vector3 position)
{
    public Matrix4 View;

    public Matrix4 Projection =
        Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(70f), 800f / 600f, 0.1f, 100f);

    private Vector3 _position = position;
    private Vector3 _front = Vector3.UnitZ;
    private readonly Vector3 _up = Vector3.UnitY;

    private float _yaw = -90f, _pitch;
    private bool _mouseCaptured;
    private Vector2 _lastMousePos;

    public void Update(KeyboardState keyboard, MouseState mouse, Vector2 mousePos, double deltaTime)
    {
        HandleCameraMovement(keyboard, deltaTime);
        HandleMouseLook(mouse, mousePos);
        UpdateMatrix();
    }

    private void UpdateMatrix()
    {
        View = Matrix4.LookAt(_position, _position + _front, _up);
    }

    private void HandleCameraMovement(KeyboardState keyboard, double deltaTime)
    {
        const float speed = 5f;
        var velocity = speed * (float)deltaTime;

        if (keyboard.IsKeyDown(Keys.W)) _position += _front * velocity;
        if (keyboard.IsKeyDown(Keys.S)) _position -= _front * velocity;
        if (keyboard.IsKeyDown(Keys.A)) _position -= Vector3.Normalize(Vector3.Cross(_front, _up)) * velocity;
        if (keyboard.IsKeyDown(Keys.D)) _position += Vector3.Normalize(Vector3.Cross(_front, _up)) * velocity;
    }

    private void HandleMouseLook(MouseState mouse, Vector2 mousePos)
    {
        if (!mouse.IsButtonDown(MouseButton.Left))
        {
            _mouseCaptured = false;
            return;
        }

        if (!_mouseCaptured)
        {
            _lastMousePos = mousePos;
            _mouseCaptured = true;
        }

        var delta = _lastMousePos - mousePos;
        _lastMousePos = mousePos;

        const float sensitivity = 0.1f;
        _yaw -= delta.X * sensitivity;
        _pitch += delta.Y * sensitivity;
        _pitch = MathHelper.Clamp(_pitch, -89f, 89f);

        var direction = new Vector3(
            MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(_pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch))
        );
        _front = Vector3.Normalize(direction);
    }
}