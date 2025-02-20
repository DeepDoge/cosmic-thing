using System.Numerics;
using System.Runtime.InteropServices;
using cosmic_thing.Camera;
using cosmic_thing.Graphics;
using cosmic_thing.Transforms;
using Friflo.Engine.ECS.Systems;

namespace cosmic_thing.Cube;

public class CubeRendererSystem : QuerySystem<LocalToWorld, ColorRgb>
{
    private const string VertexSource = """
                                        #version 460 core
                                        layout(location = 0) in vec3 vertexPosition;
                                        layout(location = 1) in vec3 instanceColor;
                                        layout(location = 2) in mat4 instanceWorld;

                                        uniform mat4 projection;
                                        uniform mat4 view;

                                        out vec3 color;

                                        void main() {
                                            vec3 vertexWorldPosition = (instanceWorld * vec4(vertexPosition, 1.0)).xyz;
                                            gl_Position = projection * view * vec4(vertexWorldPosition, 1.0);
                                        
                                            color = instanceColor;
                                        }
                                        """;

    private const string FragmentSource = """
                                          #version 460 core
                                          out vec4 fragmentColor;
                                          in vec3 color;
                                          void main() {
                                              fragmentColor = vec4(color, 1.0);
                                          }
                                          """;

    private static readonly Vector3[] Vertices =
    [
        new(-0.5f, -0.5f, -0.5f), // 0 - back bottom left
        new(0.5f, -0.5f, -0.5f), // 1 - back bottom right
        new(0.5f, 0.5f, -0.5f), // 2 - back top right
        new(-0.5f, 0.5f, -0.5f), // 3 - back top left
        new(-0.5f, -0.5f, 0.5f), // 4 - front bottom left
        new(0.5f, -0.5f, 0.5f), // 5 - front bottom right
        new(0.5f, 0.5f, 0.5f), // 6 - front top right
        new(-0.5f, 0.5f, 0.5f) // 7 - front top left
    ];

    private static readonly uint[] Indices =
    [
        0, 1, 2, 2, 3, 0, // Back face
        4, 5, 6, 6, 7, 4, // Front face
        0, 4, 7, 7, 3, 0, // Left face
        1, 5, 6, 6, 2, 1, // Right face
        3, 2, 6, 6, 7, 3, // Top face
        0, 1, 5, 5, 4, 0 // Bottom face
    ];

    private readonly DrawElementsIndirectBuffer _drawBuffer = new();
    private readonly InstanceDataBuffer<ColorRgb> _instanceColors = new(1);
    private readonly InstanceDataBuffer<LocalToWorld> _instanceWorld = new(2);
    private readonly ShaderProgram _shader = new(VertexSource, FragmentSource);
    private readonly VertexArrayObject _vao = VertexArrayObject.FromMesh(0, Vertices, Indices);

    private int _instanceCountCache;


    public CubeRendererSystem()
    {
        VertexArrayObject.Bind(_vao);
        _instanceColors.Bind();
        _instanceWorld.Bind();
        VertexArrayObject.Unbind();
    }

    private void ResizeBuffersIfNeeded()
    {
        var instanceCount = Query?.Count ?? 0;
        if (instanceCount == _instanceCountCache) return;
        _instanceCountCache = instanceCount;

        _instanceWorld.Resize(instanceCount);
        _instanceColors.Resize(instanceCount);

        _drawBuffer.Update((uint)Indices.Length, (uint)instanceCount, 0, 0, 0);
    }

    protected override void OnUpdate()
    {
        ResizeBuffersIfNeeded();

        ref var cameraData = ref Game.MainCameraEntity.GetComponent<CameraData>();

        var offset = 0;
        foreach (var (localToWorldChunk, colorChunk, entities) in Query.Chunks)
        {
            _instanceWorld.UpdateData(ref MemoryMarshal.GetReference(localToWorldChunk.Span), localToWorldChunk.Length,
                offset);
            _instanceColors.UpdateData(ref MemoryMarshal.GetReference(colorChunk.Span), colorChunk.Length, offset);
            offset += entities.Length;
        }

        _shader.Use();
        _shader.SetUniformMatrix4X4("projection", ref cameraData.Projection);
        _shader.SetUniformMatrix4X4("view", ref cameraData.View);

        VertexArrayObject.Bind(_vao);
        _drawBuffer.Draw();
        VertexArrayObject.Unbind();
    }
}