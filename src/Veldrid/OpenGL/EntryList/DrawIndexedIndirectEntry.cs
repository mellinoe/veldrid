namespace Veldrid.OpenGL.EntryList
{
    internal struct DrawIndexedIndirectEntry
    {
        public Tracked<DeviceBuffer> IndirectBuffer;
        public uint Offset;
        public uint DrawCount;
        public uint Stride;

        public DrawIndexedIndirectEntry(Tracked<DeviceBuffer> indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            IndirectBuffer = indirectBuffer;
            Offset = offset;
            DrawCount = drawCount;
            Stride = stride;
        }
    }
}
