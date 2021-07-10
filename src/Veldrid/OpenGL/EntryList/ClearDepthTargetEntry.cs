namespace Veldrid.OpenGL.EntryList
{
    internal struct ClearDepthTargetEntry
    {
        public readonly float Depth;
        public readonly byte Stencil;

        public ClearDepthTargetEntry(float depth, byte stencil)
        {
            Depth = depth;
            Stencil = stencil;
        }
    }
}
