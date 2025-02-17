using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace cosmic_thing.Graphics;

public struct ShaderProgram : IDisposable
{
    public int Id { get; private set; }

    public ShaderProgram(string vertexSource, string fragmentSource)
    {
        Id = GL.CreateProgram();

        var vertexShader = CompileShader(vertexSource, ShaderType.VertexShader);
        var fragmentShader = CompileShader(fragmentSource, ShaderType.FragmentShader);

        GL.AttachShader(Id, vertexShader);
        GL.AttachShader(Id, fragmentShader);
        GL.LinkProgram(Id);

        GL.DetachShader(Id, vertexShader);
        GL.DetachShader(Id, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    private static int CompileShader(string source, ShaderType type)
    {
        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameter.CompileStatus, out var success);
        if (success == 0) throw new Exception($"{type} Compilation Error: {GL.GetShaderInfoLog(shader)}");

        return shader;
    }

    private void EnsureNotDisposed()
    {
        if (Id == 0) throw new ObjectDisposedException(nameof(ShaderProgram), "Shader program has been deleted.");
    }

    public void Use()
    {
        EnsureNotDisposed();
        GL.UseProgram(Id);
    }

    public void SetMatrix4(string name, Matrix4x4 matrix)
    {
        EnsureNotDisposed();
        var matrix4 = Unsafe.BitCast<Matrix4x4, Matrix4>(matrix);
        var location = GL.GetUniformLocation(Id, name);
        GL.UniformMatrix4(location, false, ref matrix4);
    }

    public void Dispose()
    {
        if (Id == 0) return;
        GL.DeleteProgram(Id);
        Id = 0;
    }
}