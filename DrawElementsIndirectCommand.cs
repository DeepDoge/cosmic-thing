using System.Runtime.InteropServices;

namespace cosmic_thing;

[StructLayout(LayoutKind.Sequential)]
public struct DrawElementsIndirectCommand
{
    public uint Count;
    public uint InstanceCount;
    public uint FirstIndex;
    public int BaseVertex;
    public uint BaseInstance;
}