using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace cosmic_thing.Graphics;

public readonly struct ShaderProgram
{
    private readonly int _program;

    public ShaderProgram(string vertexSource, string fragmentSource)
    {
        _program = GL.CreateProgram();

        var vertexShader = CompileShader(vertexSource, ShaderType.VertexShader);
        var fragmentShader = CompileShader(fragmentSource, ShaderType.FragmentShader);

        GL.AttachShader(_program, vertexShader);
        GL.AttachShader(_program, fragmentShader);
        GL.LinkProgram(_program);

        GL.DetachShader(_program, vertexShader);
        GL.DetachShader(_program, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
        return;

        int CompileShader(string source, ShaderType type)
        {
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var success);
            if (success == 0) throw new Exception($"{type} Compilation Error: {GL.GetShaderInfoLog(shader)}");

            return shader;
        }
    }

    public void Use()
    {
        GL.UseProgram(_program);
    }

    public void SetUniformMatrix4X4(string name, ref Matrix4x4 matrix)
    {
        var location = GL.GetUniformLocation(_program, name);
        ref var matrix4 = ref Unsafe.As<Matrix4x4, Matrix4>(ref matrix);
        GL.UniformMatrix4(location, false, ref matrix4);
    }
}