using System;

namespace Veldrid
{
    /// <summary>
    /// A <see cref="Pipeline"/> component describing the properties of the rasterizer.
    /// </summary>
    public struct RasterizerStateDescription : IEquatable<RasterizerStateDescription>
    {
        /// <summary>
        /// Controls which face will be culled.
        /// </summary>
        public FaceCullMode CullMode;
        /// <summary>
        /// Controls how the rasterizer fills polygons.
        /// </summary>
        public PolygonFillMode FillMode;
        /// <summary>
        /// Controls the winding order used to determine the front face of primitives.
        /// </summary>
        public FrontFace FrontFace;
        /// <summary>
        /// Controls whether depth clipping is enabled.
        /// </summary>
        public bool DepthClipEnabled;
        /// <summary>
        /// Controls whether the scissor test is enabled.
        /// </summary>
        public bool ScissorTestEnabled;
        /// <summary>
        /// Controls whether depth bias is enabled.
        /// </summary>
        public bool DepthBiasEnabled;
        /// <summary>
        /// Controls how much constant depth bias to apply.
        /// </summary>
        public int DepthBiasConstant;
        /// <summary>
        /// Controls how much slope-scaled depth bias to apply.
        /// </summary>
        public float DepthBiasSlopeScaled;

        /// <summary>
        /// Constructs a new RasterizerStateDescription.
        /// </summary>
        /// <param name="cullMode">Controls which face will be culled.</param>
        /// <param name="fillMode">Controls how the rasterizer fills polygons.</param>
        /// <param name="frontFace">Controls the winding order used to determine the front face of primitives.</param>
        /// <param name="depthClipEnabled">Controls whether depth clipping is enabled.</param>
        /// <param name="scissorTestEnabled">Controls whether the scissor test is enabled.</param>
        /// <param name="depthBiasEnabled">Controls whether depth bias is enabled.</param>
        /// <param name="depthBiasConstant">Controls how much constant depth bias to apply.</param>
        /// <param name="depthBiasSlope">Controls how much slope-scaled depth bias to apply.</param>
        public RasterizerStateDescription(
            FaceCullMode cullMode,
            PolygonFillMode fillMode,
            FrontFace frontFace,
            bool depthClipEnabled,
            bool scissorTestEnabled,
            bool depthBiasEnabled = false,
            int depthBiasConstant = 0,
            float depthBiasSlope = 0)
        {
            CullMode = cullMode;
            FillMode = fillMode;
            FrontFace = frontFace;
            DepthClipEnabled = depthClipEnabled;
            ScissorTestEnabled = scissorTestEnabled;
            DepthBiasEnabled = depthBiasEnabled;
            DepthBiasSlopeScaled = depthBiasSlope;
            DepthBiasConstant = depthBiasConstant;
        }

        /// <summary>
        /// Describes the default rasterizer state, with clockwise backface culling, solid polygon filling, and both depth
        /// clipping and scissor tests enabled.
        /// Settings:
        ///     CullMode = FaceCullMode.Back
        ///     FillMode = PolygonFillMode.Solid
        ///     FrontFace = FrontFace.Clockwise
        ///     DepthClipEnabled = true
        ///     ScissorTestEnabled = false
        ///     DepthBiasEnabled = false
        ///     DepthBiasSlopeScaled = 0
        ///     DepthBiasConstant = 0
        /// </summary>
        public static readonly RasterizerStateDescription Default = new RasterizerStateDescription
        {
            CullMode = FaceCullMode.Back,
            FillMode = PolygonFillMode.Solid,
            FrontFace = FrontFace.Clockwise,
            DepthClipEnabled = true,
            ScissorTestEnabled = false,
            DepthBiasEnabled = false,
            DepthBiasSlopeScaled = 0,
            DepthBiasConstant = 0,
        };

        /// <summary>
        /// Describes a rasterizer state with no culling, solid polygon filling, and both depth
        /// clipping and scissor tests enabled.
        /// Settings:
        ///     CullMode = FaceCullMode.None
        ///     FillMode = PolygonFillMode.Solid
        ///     FrontFace = FrontFace.Clockwise
        ///     DepthClipEnabled = true
        ///     ScissorTestEnabled = false
        ///     DepthBiasEnabled = false
        ///     DepthBiasSlopeScaled = 0
        ///     DepthBiasConstant = 0
        /// </summary>
        public static readonly RasterizerStateDescription CullNone = new RasterizerStateDescription
        {
            CullMode = FaceCullMode.None,
            FillMode = PolygonFillMode.Solid,
            FrontFace = FrontFace.Clockwise,
            DepthClipEnabled = true,
            ScissorTestEnabled = false,
            DepthBiasEnabled = false,
            DepthBiasSlopeScaled = 0,
            DepthBiasConstant = 0,
        };

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(RasterizerStateDescription other)
        {
            return CullMode == other.CullMode
                && FillMode == other.FillMode
                && FrontFace == other.FrontFace
                && DepthClipEnabled.Equals(other.DepthClipEnabled)
                && ScissorTestEnabled.Equals(other.ScissorTestEnabled)
                && DepthBiasEnabled == other.DepthBiasEnabled
                && DepthBiasSlopeScaled == other.DepthBiasSlopeScaled
                && DepthBiasConstant == other.DepthBiasConstant;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                (int)CullMode,
                (int)FillMode,
                (int)FrontFace,
                DepthClipEnabled.GetHashCode(),
                ScissorTestEnabled.GetHashCode(),
                DepthBiasEnabled.GetHashCode(),
                DepthBiasSlopeScaled.GetHashCode(),
                DepthBiasConstant.GetHashCode());
        }
    }
}
