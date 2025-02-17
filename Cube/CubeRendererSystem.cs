using System.Runtime.InteropServices;
using cosmic_thing.Camera;
using cosmic_thing.Graphics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using OpenTK.Graphics.OpenGL4;
using Vector3 = System.Numerics.Vector3;

namespace cosmic_thing.Cube;

// Later send the matrix
public class CubeRendererSystem : QuerySystem<Position, ColorRgb>
{
    private readonly InstanceDataBuffer<ColorRgb> _instanceColors = new(2, 3);
    private readonly InstanceDataBuffer<Position> _instanceOffsets = new(1, 3);

    private readonly Mesh _mesh = new(
        [
            new Vector3(-0.5f, -0.5f, -0.5f), // 0 - back bottom left
            new Vector3(0.5f, -0.5f, -0.5f), // 1 - back bottom right
            new Vector3(0.5f, 0.5f, -0.5f), // 2 - back top right
            new Vector3(-0.5f, 0.5f, -0.5f), // 3 - back top left
            new Vector3(-0.5f, -0.5f, 0.5f), // 4 - front bottom left
            new Vector3(0.5f, -0.5f, 0.5f), // 5 - front bottom right
            new Vector3(0.5f, 0.5f, 0.5f), // 6 - front top right
            new Vector3(-0.5f, 0.5f, 0.5f) // 7 - front top left
        ],
        [
            0, 1, 2, 2, 3, 0, // Back face
            4, 5, 6, 6, 7, 4, // Front face
            0, 4, 7, 7, 3, 0, // Left face
            1, 5, 6, 6, 2, 1, // Right face
            3, 2, 6, 6, 7, 3, // Top face
            0, 1, 5, 5, 4, 0 // Bottom face
        ]
    );

    private int _instanceCountCache;

    private ShaderProgram _shader = new("""
                                        #version 450 core
                                        layout(location = 0) in vec3 vertexPosition;
                                        layout(location = 1) in vec3 instanceOffset;
                                        layout(location = 2) in vec3 instanceColor;
                                        uniform mat4 projection;
                                        uniform mat4 view;
                                        out vec3 color;
                                        void main() {
                                            gl_Position = projection * view * vec4(vertexPosition + instanceOffset, 1.0);
                                            color = instanceColor;
                                        }
                                        """, """
                                             #version 450 core
                                             out vec4 fragmentColor;
                                             in vec3 color;
                                             void main() {
                                                 fragmentColor = vec4(color, 1.0);
                                             }
                                             """);

    private void ResizeBuffersIfNeeded()
    {
        var instanceCount = Query?.Count ?? 0;
        if (instanceCount == _instanceCountCache) return;
        _instanceCountCache = instanceCount;

        _instanceOffsets.Resize(instanceCount);
        _instanceColors.Resize(instanceCount);
    }

    protected override void OnUpdate()
    {
        ResizeBuffersIfNeeded();

        ref var cameraData = ref Game.MainCameraEntity.GetComponent<CameraData>();

        var colorOffset = 0;
        var positionOffset = 0;
        foreach (var entityChunk in Query.Chunks)
        {
            entityChunk.Deconstruct(out var position, out var color, out _);

            _instanceOffsets.UpdateData(ref MemoryMarshal.GetReference(position.Span), position.Length,
                positionOffset);
            positionOffset += position.Length;
            _instanceColors.UpdateData(ref MemoryMarshal.GetReference(color.Span), color.Length, colorOffset);
            colorOffset += color.Length;
        }

        _shader.Use();
        _shader.SetUniformMatrix4X4("projection", ref cameraData.Projection);
        _shader.SetUniformMatrix4X4("view", ref cameraData.View);

        _mesh.Bind();
        _instanceOffsets.Bind();
        _instanceColors.Bind();

        GL.DrawElementsInstanced(PrimitiveType.Triangles, _mesh.IndicesLength, DrawElementsType.UnsignedInt,
            IntPtr.Zero,
            _instanceCountCache);
    }
}