namespace Veldrid.OpenGL.EntryList
{
    internal struct SetViewportEntry
    {
        public readonly uint Index;
        public Viewport Viewport;

        public SetViewportEntry(uint index, ref Viewport viewport)
        {
            Index = index;
            Viewport = viewport;
        }
    }
}
