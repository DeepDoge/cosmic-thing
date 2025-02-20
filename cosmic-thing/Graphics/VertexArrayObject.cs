using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace cosmic_thing.Graphics;

public readonly struct VertexArrayObject()
{
    private readonly int _id = GL.GenVertexArray();

    public static VertexArrayObject FromMesh(int location, Vector3[] vertices, uint[] indices)
    {
        var vao = new VertexArrayObject();

        Bind(vao);

        var vbo = GL.GenBuffer();
        var ebo = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float) * 3, vertices,
            BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(location, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        GL.EnableVertexAttribArray(0);

        Unbind();

        return vao;
    }

    public static void Bind(VertexArrayObject vao)
    {
        GL.BindVertexArray(vao._id);
    }

    public static void Unbind()
    {
        GL.BindVertexArray(0);
    }
}