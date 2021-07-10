namespace Veldrid.OpenGL.EntryList
{
    internal struct SetFramebufferEntry
    {
        public readonly Tracked<Framebuffer> Framebuffer;

        public SetFramebufferEntry(Tracked<Framebuffer> fb)
        {
            Framebuffer = fb;
        }
    }
}
