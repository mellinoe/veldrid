namespace Veldrid.OpenGL.EntryList
{
    internal struct SetVertexBufferEntry
    {
        public readonly uint Index;
        public readonly Tracked<DeviceBuffer> Buffer;
        public uint Offset;

        public SetVertexBufferEntry(uint index, Tracked<DeviceBuffer> buffer, uint offset)
        {
            Index = index;
            Buffer = buffer;
            Offset = offset;
        }
    }
}
