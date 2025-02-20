using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;

namespace cosmic_thing.Graphics;

public readonly struct GraphicsArrayBuffer<T>()
    where T : struct
{
    private const BufferTarget Target = BufferTarget.ArrayBuffer;
    private static readonly int Stride = Unsafe.SizeOf<T>();

    private readonly int _buffer = GL.GenBuffer();

    public void BufferData(BufferUsageHint usage, int length)
    {
        GL.BindBuffer(Target, _buffer);
        GL.BufferData(Target, length * Stride, IntPtr.Zero, usage);
    }

    public void BufferSubData(ref T data, int length, int indexOffset = 0)
    {
        GL.BindBuffer(Target, _buffer);
        GL.BufferSubData(Target, indexOffset * Stride, length * Stride, ref data);
    }
}