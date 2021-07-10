namespace Veldrid.OpenGL.EntryList
{
    internal struct SetPipelineEntry
    {
        public readonly Tracked<Pipeline> Pipeline;

        public SetPipelineEntry(Tracked<Pipeline> pipeline)
        {
            Pipeline = pipeline;
        }
    }
}
