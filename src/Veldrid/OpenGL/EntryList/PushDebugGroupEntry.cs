namespace Veldrid.OpenGL.EntryList
{
    internal struct PushDebugGroupEntry
    {
        public Tracked<string> Name;

        public PushDebugGroupEntry(Tracked<string> name)
        {
            Name = name;
        }
    }
}
