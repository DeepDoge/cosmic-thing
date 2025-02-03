using System.Runtime.InteropServices;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace cosmic_thing;

public struct CubeTag : ITag;

public class CubeRenderer : QuerySystem<Position>
{
    private int _vao, _vbo, _instanceVbo, _shaderProgram;
    public required Camera Camera;

    public CubeRenderer() => Filter.AnyTags(Tags.Get<CubeTag>());


    protected override void OnAddStore(EntityStore store)
    {
        // Setup OpenGL buffers
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _instanceVbo = GL.GenBuffer();

        float[] vertices =
        [
            0.0f, 0.5f, 0.0f, // Top
            -0.5f, -0.5f, 0.0f, // Left
            0.5f, -0.5f, 0.0f // Right
        ];

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, 100 * Vector3.SizeInBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);

        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribDivisor(1, 1);

        // ✅ Correctly compile shaders
        _shaderProgram = CompileShader();
    }

    protected override void OnUpdate()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVbo);

        nint offset = 0;
        foreach (var chunk in Query.Chunks)
        {
            var size = chunk.Length * Vector3.SizeInBytes;
            GL.BufferSubData(BufferTarget.ArrayBuffer, offset, size, ref MemoryMarshal.GetReference(chunk.Chunk1.Span));

            offset += size;
        }

        var count = Query.Count;
        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vao);

        // Send Matrices to Shader
        var projectionLocation = GL.GetUniformLocation(_shaderProgram, "projection");
        var viewLocation = GL.GetUniformLocation(_shaderProgram, "view");
        GL.UniformMatrix4(projectionLocation, false, ref Camera.Projection);
        GL.UniformMatrix4(viewLocation, false, ref Camera.View);

        GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 3, count);
    }
    
    private static int CompileShader()
    {
        const string vertexShaderSource = """
                                          #version 450 core
                                          layout(location = 0) in vec3 position;
                                          layout(location = 1) in vec3 instanceOffset;
                                          uniform mat4 projection;
                                          uniform mat4 view;
                                          void main() {
                                              gl_Position = projection * view * vec4(position + instanceOffset, 1.0);
                                          }
                                          """;

        const string fragmentShaderSource = """
                                            #version 450 core
                                            out vec4 FragColor;
                                            void main() {
                                                FragColor = vec4(1.0, 0.0, 0.0, 1.0);
                                            }
                                            """;

        // Compile Vertex Shader
        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);
        CheckShaderCompile(vertexShader, "Vertex Shader");

        // Compile Fragment Shader
        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);
        CheckShaderCompile(fragmentShader, "Fragment Shader");

        // Link shaders into program
        var shaderProgram = GL.CreateProgram();
        GL.AttachShader(shaderProgram, vertexShader);
        GL.AttachShader(shaderProgram, fragmentShader);
        GL.LinkProgram(shaderProgram);

        // Cleanup shaders
        GL.DetachShader(shaderProgram, vertexShader);
        GL.DetachShader(shaderProgram, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        return shaderProgram;
    }
    
    private static void CheckShaderCompile(int shader, string shaderName)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var success);
        if (success != 0) return;
        var infoLog = GL.GetShaderInfoLog(shader);
        throw new Exception($"{shaderName} Compilation Failed: {infoLog}");
    }
}