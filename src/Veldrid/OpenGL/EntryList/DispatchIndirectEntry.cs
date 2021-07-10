namespace Veldrid.OpenGL.EntryList
{
    internal struct DispatchIndirectEntry
    {
        public Tracked<DeviceBuffer> IndirectBuffer;
        public uint Offset;

        public DispatchIndirectEntry(Tracked<DeviceBuffer> indirectBuffer, uint offset)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
        }
    }
}
