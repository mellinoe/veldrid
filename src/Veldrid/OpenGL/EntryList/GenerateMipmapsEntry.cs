namespace Veldrid.OpenGL.EntryList
{
    internal struct GenerateMipmapsEntry
    {
        public readonly Tracked<Texture> Texture;

        public GenerateMipmapsEntry(Tracked<Texture> texture)
        {
            Texture = texture;
        }
    }
}
