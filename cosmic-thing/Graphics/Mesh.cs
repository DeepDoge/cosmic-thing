using System.Numerics;
using OpenTK.Graphics.OpenGL4;

namespace cosmic_thing.Graphics;

public readonly struct Mesh
{
    public readonly uint IndicesLength;
    private readonly int _vao;

    public Mesh(Vector3[] vertices, uint[] indices)
    {
        // generate buffers
        _vao = GL.GenVertexArray();
        var vbo = GL.GenBuffer();
        var ebo = GL.GenBuffer();
        IndicesLength = (uint)indices.Length;

        GL.BindVertexArray(_vao);

        // upload vertex data
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float) * 3, vertices,
            BufferUsageHint.StaticDraw);

        // upload index data
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices,
            BufferUsageHint.StaticDraw);

        // define vertex layout (position only for now)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0); // unbind
    }

    public void Bind()
    {
        GL.BindVertexArray(_vao);
    }
}