using System;

namespace Veldrid.OpenGL.EntryList
{
    internal unsafe struct SetResourceSetEntry
    {
        public const int MaxInlineDynamicOffsets = 10;

        public readonly uint Slot;
        public readonly Tracked<ResourceSet> ResourceSet;
        public readonly bool IsGraphics;
        public readonly uint DynamicOffsetCount;
        public fixed uint DynamicOffsets_Inline[MaxInlineDynamicOffsets];
        public readonly StagingBlock DynamicOffsets_Block;

        public SetResourceSetEntry(
            uint slot,
            Tracked<ResourceSet> rs,
            bool isGraphics,
            ReadOnlySpan<uint> dynamicOffsets)
        {
            Slot = slot;
            ResourceSet = rs;
            IsGraphics = isGraphics;
            DynamicOffsetCount = (uint)dynamicOffsets.Length;
            for (int i = 0; i < dynamicOffsets.Length; i++)
            {
                DynamicOffsets_Inline[i] = dynamicOffsets[i];
            }

            DynamicOffsets_Block = default;
        }

        public SetResourceSetEntry(
            uint slot,
            Tracked<ResourceSet> rs,
            bool isGraphics,
            StagingBlock dynamicOffsets)
        {
            Slot = slot;
            ResourceSet = rs;
            IsGraphics = isGraphics;
            DynamicOffsetCount = dynamicOffsets.SizeInBytes / sizeof(uint);
            DynamicOffsets_Block = dynamicOffsets;
        }
    }
}
