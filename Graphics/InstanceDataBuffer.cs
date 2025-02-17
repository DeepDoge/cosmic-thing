using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace cosmic_thing.Graphics;

public readonly struct InstanceDataBuffer<T> where T : struct
{
    private readonly int _attributeLocation;
    private readonly int _bufferId;
    private readonly int _componentCount;
    private readonly int _stride = Marshal.SizeOf<T>();
    private readonly BufferTarget _target;

    public InstanceDataBuffer(int attributeLocation, int componentCount, BufferTarget target = BufferTarget.ArrayBuffer)
    {
        _attributeLocation = attributeLocation;
        _componentCount = componentCount;
        _target = target;

        _bufferId = GL.GenBuffer();
        GL.BindBuffer(target, _bufferId);

        // initial empty buffer, will be resized later
        Resize(0);
    }

    public void Resize(int length)
    {
        GL.BindBuffer(_target, _bufferId);
        GL.BufferData(_target, length * _stride, nint.Zero, BufferUsageHint.DynamicDraw);
    }

    public void UpdateDataAndResize(ref T data, int length)
    {
        GL.BindBuffer(_target, _bufferId);
        GL.BufferData(_target, length * _stride, ref data, BufferUsageHint.DynamicDraw);
    }

    public void UpdateData(ref T data, int length, int indexOffset = 0)
    {
        GL.BindBuffer(_target, _bufferId);
        GL.BufferSubData(_target, (nint)indexOffset * _stride, length * _stride, ref data);
    }

    public void Bind()
    {
        GL.BindBuffer(_target, _bufferId);
        GL.VertexAttribPointer(_attributeLocation, _componentCount, VertexAttribPointerType.Float, false,
            _componentCount * sizeof(float), 0);
        GL.EnableVertexAttribArray(_attributeLocation);
        GL.VertexAttribDivisor(_attributeLocation, 1);
    }
}