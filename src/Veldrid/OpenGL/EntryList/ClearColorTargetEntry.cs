namespace Veldrid.OpenGL.EntryList
{
    internal struct ClearColorTargetEntry
    {
        public readonly uint Index;
        public readonly RgbaFloat ClearColor;

        public ClearColorTargetEntry(uint index, RgbaFloat clearColor)
        {
            Index = index;
            ClearColor = clearColor;
        }
    }
}
