namespace Veldrid.OpenGL.EntryList
{
    internal struct InsertDebugMarkerEntry
    {
        public Tracked<string> Name;

        public InsertDebugMarkerEntry(Tracked<string> name)
        {
            Name = name;
        }
    }
}
