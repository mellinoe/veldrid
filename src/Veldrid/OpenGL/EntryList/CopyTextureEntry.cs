namespace Veldrid.OpenGL.EntryList
{
    internal struct CopyTextureEntry
    {
        public readonly Tracked<Texture> Source;
        public readonly uint SrcX;
        public readonly uint SrcY;
        public readonly uint SrcZ;
        public readonly uint SrcMipLevel;
        public readonly uint SrcBaseArrayLayer;
        public readonly Tracked<Texture> Destination;
        public readonly uint DstX;
        public readonly uint DstY;
        public readonly uint DstZ;
        public readonly uint DstMipLevel;
        public readonly uint DstBaseArrayLayer;
        public readonly uint Width;
        public readonly uint Height;
        public readonly uint Depth;
        public readonly uint LayerCount;

        public CopyTextureEntry(
            Tracked<Texture> source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel,
            uint srcBaseArrayLayer,
            Tracked<Texture> destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel,
            uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount)
        {
            Source = source;
            SrcX = srcX;
            SrcY = srcY;
            SrcZ = srcZ;
            SrcMipLevel = srcMipLevel;
            SrcBaseArrayLayer = srcBaseArrayLayer;
            Destination = destination;
            DstX = dstX;
            DstY = dstY;
            DstZ = dstZ;
            DstMipLevel = dstMipLevel;
            DstBaseArrayLayer = dstBaseArrayLayer;
            Width = width;
            Height = height;
            Depth = depth;
            LayerCount = layerCount;
        }
    }
}
