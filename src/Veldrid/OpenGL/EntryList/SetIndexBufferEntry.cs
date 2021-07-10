namespace Veldrid.OpenGL.EntryList
{
    internal struct SetIndexBufferEntry
    {
        public readonly Tracked<DeviceBuffer> Buffer;
        public IndexFormat Format;
        public uint Offset;

        public SetIndexBufferEntry(Tracked<DeviceBuffer> ib, IndexFormat format, uint offset)
        {
            Buffer = ib;
            Format = format;
            Offset = offset;
        }
    }
}
