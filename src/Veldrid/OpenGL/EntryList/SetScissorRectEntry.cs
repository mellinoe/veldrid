namespace Veldrid.OpenGL.EntryList
{
    internal struct SetScissorRectEntry
    {
        public readonly uint Index;
        public readonly uint X;
        public readonly uint Y;
        public readonly uint Width;
        public readonly uint Height;

        public SetScissorRectEntry(uint index, uint x, uint y, uint width, uint height)
        {
            Index = index;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
