namespace Veldrid.OpenGL.EntryList
{
    internal struct ResolveTextureEntry
    {
        public readonly Tracked<Texture> Source;
        public readonly Tracked<Texture> Destination;

        public ResolveTextureEntry(Tracked<Texture> source, Tracked<Texture> destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}
