using System.Runtime.InteropServices;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using OpenTK.Graphics.OpenGL4;
using System.Numerics;

namespace cosmic_thing;

public struct CubeTag : ITag;

public struct CubeColor : IComponent
{
    public Vector3 Value;
}

public class CubeRenderer : QuerySystem<Position, CubeColor>
{
    private static readonly Vector3[] CubeVertices =
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

    private static readonly uint[] CubeIndices =
    [
        0, 1, 2, 2, 3, 0, // Back face
        4, 5, 6, 6, 7, 4, // Front face
        0, 4, 7, 7, 3, 0, // Left face
        1, 5, 6, 6, 2, 1, // Right face
        3, 2, 6, 6, 7, 3, // Top face
        0, 1, 5, 5, 4, 0 // Bottom face
    ];

    private int _vertexArrayObject;
    private int _vertexBufferObject;
    private int _instanceOffsetBufferObject;
    private int _instanceColorBufferObject;
    private int _shaderProgramId;
    private int _drawCommandBuffer;

    public required Camera MainCamera;

    public CubeRenderer() => Filter.AnyTags(Tags.Get<CubeTag>());

    protected override void OnAddStore(EntityStore store)
    {
        // Generate a Vertex Array Object (VAO) to store the vertex attribute configuration.
        _vertexArrayObject = GL.GenVertexArray();

        // Generate the buffer that will hold the cube's vertex positions.
        _vertexBufferObject = GL.GenBuffer();

        // Generate the buffer that will hold per-instance data (for example, position offsets).
        _instanceOffsetBufferObject = GL.GenBuffer();
        _instanceColorBufferObject = GL.GenBuffer();

        // Generate the buffer that will hold the indirect draw command parameters.
        _drawCommandBuffer = GL.GenBuffer();

        // Generate the Element Buffer Object (EBO) for storing cube indices (defines the cube's faces).
        var elementBufferObject = GL.GenBuffer();

        // --- Setup the Indirect Draw Command Buffer ---
        // Bind the draw command buffer to the DrawIndirectBuffer target.
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, _drawCommandBuffer);
        // Allocate storage for one DrawElementsIndirectCommand structure.
        GL.BufferData(
            BufferTarget.DrawIndirectBuffer,
            Marshal.SizeOf<DrawElementsIndirectCommand>(),
            IntPtr.Zero,
            BufferUsageHint.DynamicDraw
        );

        // --- Bind the VAO ---
        // Bind the VAO so that all subsequent vertex attribute configuration calls are stored in it.
        GL.BindVertexArray(_vertexArrayObject);

        // --- Setup the Vertex Buffer ---
        // Bind the vertex buffer to the ARRAY_BUFFER target.
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        // Upload the cube vertices data into the vertex buffer.
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            CubeVertices.Length * sizeof(float) * 3, // 3 floats per vertex (x, y, z)
            CubeVertices,
            BufferUsageHint.StaticDraw
        );

        {
            // --- Setup the Element Buffer ---
            // Bind the element (index) buffer to the ELEMENT_ARRAY_BUFFER target.
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            // Upload the cube indices data into the element buffer.
            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                CubeIndices.Length * sizeof(uint),
                CubeIndices,
                BufferUsageHint.StaticDraw
            );

            // --- Configure Vertex Attributes for the Cube Vertices ---
            // Specify the layout of the vertex data for attribute location 0.
            // (3 floats per vertex, no normalization, tightly packed, starting at offset 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            // Enable the vertex attribute at location 0.
            GL.EnableVertexAttribArray(0);
        }

        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceOffsetBufferObject);
            // --- Configure Vertex Attributes for Instance Data ---
            // Specify the layout of the per-instance data for attribute location 1.
            // (3 floats per instance, no normalization, tightly packed, starting at offset 0)
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            // Enable the vertex attribute at location 1.
            GL.EnableVertexAttribArray(1);
            // Tell OpenGL to update attribute 1 once per instance (instead of once per vertex).
            GL.VertexAttribDivisor(1, 1);
        }

        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceColorBufferObject);

            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribDivisor(2, 1);   
        }
        
        ReAllocateInstanceBuffer();
        

        // --- Setup the Instance Buffer ---
        // This function ensures the instance buffer is allocated with the correct size.

        // --- Compile and Link the Shaders ---
        // Compile the vertex and fragment shaders, link them into a shader program, and store its ID.
        _shaderProgramId = CompileAndLinkShaders();
    }

    private int _instanceCountCache = -1;

    private void ReAllocateInstanceBuffer()
    {
        var instanceCount = Query?.Count ?? 0;
        if (instanceCount == _instanceCountCache) return;
        _instanceCountCache = instanceCount;

        {
            var instanceOffsetSize = instanceCount * 3 * sizeof(float);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceOffsetBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, instanceOffsetSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }

        {
            var instanceColorSize = instanceCount * 3 * sizeof(float);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceColorBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, instanceColorSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }
     
        
        var drawCommand = new DrawElementsIndirectCommand
        {
            Count = (uint)CubeIndices.Length,
            InstanceCount = (uint)instanceCount,
            FirstIndex = 0,
            BaseVertex = 0,
            BaseInstance = 0
        };

        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, _drawCommandBuffer);
        GL.BufferSubData(BufferTarget.DrawIndirectBuffer, IntPtr.Zero, Marshal.SizeOf<DrawElementsIndirectCommand>(),
            ref drawCommand);
    }

    protected override void OnUpdate()
    {
        ReAllocateInstanceBuffer();
        
        nint colorBufferOffset = 0;
        nint positionBufferOffset = 0;
        foreach (var entityChunk in Query.Chunks)
        {
            entityChunk.Deconstruct(out var position, out var color, out _);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceOffsetBufferObject);
            var positionBufferChunkSize = position.Length * sizeof(float) * 3;
            GL.BufferSubData(BufferTarget.ArrayBuffer, positionBufferOffset, positionBufferChunkSize,
                ref MemoryMarshal.GetReference(position.Span));
            positionBufferOffset += positionBufferChunkSize;
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceColorBufferObject);
            var colorBufferChunkSize = color.Length * sizeof(float) * 3;
            GL.BufferSubData(BufferTarget.ArrayBuffer, colorBufferOffset, colorBufferChunkSize,
                ref MemoryMarshal.GetReference(color.Span));
            colorBufferOffset += colorBufferChunkSize;
        }


        GL.UseProgram(_shaderProgramId);
        GL.BindVertexArray(_vertexArrayObject);

        var projectionMatrixLocation = GL.GetUniformLocation(_shaderProgramId, "projection");
        var viewMatrixLocation = GL.GetUniformLocation(_shaderProgramId, "view");
        GL.UniformMatrix4(projectionMatrixLocation, false, ref MainCamera.Projection);
        GL.UniformMatrix4(viewMatrixLocation, false, ref MainCamera.View);

        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, _drawCommandBuffer);
        GL.DrawElementsIndirect(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, IntPtr.Zero);
    }


    private static int CompileAndLinkShaders()
    {
        const string vertexShaderSource = """
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
                                          """;

        const string fragmentShaderSource = """
                                            #version 450 core
                                            out vec4 fragmentColor;
                                            in vec3 color;
                                            void main() {
                                                fragmentColor = vec4(color, 1.0);
                                            }
                                            """;

        // Compile and check vertex shader
        var vertexShaderId = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShaderId, vertexShaderSource);
        GL.CompileShader(vertexShaderId);
        ValidateShaderCompilation(vertexShaderId, "Vertex Shader");

        // Compile and check fragment shader
        var fragmentShaderId = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShaderId, fragmentShaderSource);
        GL.CompileShader(fragmentShaderId);
        ValidateShaderCompilation(fragmentShaderId, "Fragment Shader");

        // Link shaders into a shader program
        var shaderProgramId = GL.CreateProgram();
        GL.AttachShader(shaderProgramId, vertexShaderId);
        GL.AttachShader(shaderProgramId, fragmentShaderId);
        GL.LinkProgram(shaderProgramId);

        // Cleanup shader objects
        GL.DetachShader(shaderProgramId, vertexShaderId);
        GL.DetachShader(shaderProgramId, fragmentShaderId);
        GL.DeleteShader(vertexShaderId);
        GL.DeleteShader(fragmentShaderId);

        return shaderProgramId;
    }

    private static void ValidateShaderCompilation(int shaderId, string shaderType)
    {
        GL.GetShader(shaderId, ShaderParameter.CompileStatus, out var isCompiled);
        if (isCompiled != 0) return;
        var shaderErrorLog = GL.GetShaderInfoLog(shaderId);
        throw new Exception($"{shaderType} Compilation Failed: {shaderErrorLog}");
    }
}