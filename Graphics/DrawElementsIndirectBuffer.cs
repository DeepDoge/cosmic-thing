using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace cosmic_thing.Graphics;

public readonly struct DrawElementsIndirectBuffer
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Data
    {
        public required uint Count;
        public required uint InstanceCount;
        public required uint FirstIndex;
        public required int BaseVertex;
        public required uint BaseInstance;
    }

    private static readonly int Stride = Marshal.SizeOf<Data>();
    private readonly int _buffer;

    public DrawElementsIndirectBuffer()
    {
        _buffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, _buffer);
        GL.BufferData(BufferTarget.DrawIndirectBuffer, Stride, IntPtr.Zero, BufferUsageHint.DynamicDraw);
    }

    public void Update(uint count, uint instanceCount, uint firstIndex, int baseVertex, uint baseInstance)
    {
        var data = new Data
        {
            Count = count,
            InstanceCount = instanceCount,
            FirstIndex = firstIndex,
            BaseVertex = baseVertex,
            BaseInstance = baseInstance
        };

        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, _buffer);
        GL.BufferSubData(BufferTarget.DrawIndirectBuffer, IntPtr.Zero, Stride, ref data);
    }

    public void Draw()
    {
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, _buffer);
        GL.DrawElementsIndirect(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, IntPtr.Zero);
    }
}