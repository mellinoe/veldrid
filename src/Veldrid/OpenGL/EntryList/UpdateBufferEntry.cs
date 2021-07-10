namespace Veldrid.OpenGL.EntryList
{
    internal struct UpdateBufferEntry
    {
        public readonly Tracked<DeviceBuffer> Buffer;
        public readonly uint BufferOffsetInBytes;
        public readonly StagingBlock StagingBlock;
        public readonly uint StagingBlockSize;

        public UpdateBufferEntry(Tracked<DeviceBuffer> buffer, uint bufferOffsetInBytes, StagingBlock stagingBlock, uint stagingBlockSize)
        {
            Buffer = buffer;
            BufferOffsetInBytes = bufferOffsetInBytes;
            StagingBlock = stagingBlock;
            StagingBlockSize = stagingBlockSize;
        }
    }
}
