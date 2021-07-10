namespace Veldrid.OpenGL.EntryList
{
    internal struct DispatchEntry
    {
        public uint GroupCountX;
        public uint GroupCountY;
        public uint GroupCountZ;

        public DispatchEntry(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            GroupCountX = groupCountX;
            GroupCountY = groupCountY;
            GroupCountZ = groupCountZ;
        }
    }
}
