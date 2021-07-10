namespace Veldrid.OpenGL.EntryList
{
    internal struct DrawEntry
    {
        public uint VertexCount;
        public uint InstanceCount;
        public uint VertexStart;
        public uint InstanceStart;

        public DrawEntry(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            VertexCount = vertexCount;
            InstanceCount = instanceCount;
            VertexStart = vertexStart;
            InstanceStart = instanceStart;
        }
    }
}
