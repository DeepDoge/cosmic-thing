using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace cosmic_thing.Graphics;

public readonly struct InstanceDataBuffer<T> where T : struct
{
    private static readonly int Stride = Marshal.SizeOf<T>();

    private readonly int _attributeLocation;
    private readonly int _bufferId;
    private readonly BufferTarget _target;

    public InstanceDataBuffer(int attributeLocation, BufferTarget target = BufferTarget.ArrayBuffer)
    {
        _attributeLocation = attributeLocation;
        _target = target;

        _bufferId = GL.GenBuffer();
        Resize(0);
    }

    public void Resize(int length)
    {
        GL.BindBuffer(_target, _bufferId);
        GL.BufferData(_target, length * Stride, nint.Zero, BufferUsageHint.DynamicDraw);
    }

    public void UpdateDataAndResize(ref T data, int length)
    {
        GL.BindBuffer(_target, _bufferId);
        GL.BufferData(_target, length * Stride, ref data, BufferUsageHint.DynamicDraw);
    }

    public void UpdateData(ref T data, int length, int indexOffset = 0)
    {
        GL.BindBuffer(_target, _bufferId);
        GL.BufferSubData(_target, (nint)indexOffset * Stride, length * Stride, ref data);
    }

    public void Bind()
    {
        GL.BindBuffer(_target, _bufferId);
        const VertexAttribPointerType componentType = VertexAttribPointerType.Float;
        var componentSize = Stride / sizeof(float);
        if (componentSize <= 4)
        {
            GL.EnableVertexAttribArray(_attributeLocation);
            GL.VertexAttribDivisor(_attributeLocation, 1);
            GL.VertexAttribPointer(_attributeLocation, componentSize, componentType, false, Stride, 0);
        }
        else
        {
            const int floatsPerVec4 = 4;
            const int floatSizeInBytes = sizeof(float);
            var offset = 0;
            var subLocation = 0;

            for (var remaining = componentSize; remaining > 0; remaining -= floatsPerVec4)
            {
                var location = _attributeLocation + subLocation;
                var subComponentSize = Math.Min(floatsPerVec4, remaining);

                GL.EnableVertexAttribArray(location);
                GL.VertexAttribDivisor(location, 1);
                GL.VertexAttribPointer(location, subComponentSize, componentType, false, Stride, offset);

                offset += floatsPerVec4 * floatSizeInBytes;
                subLocation++;
            }
        }
    }
}