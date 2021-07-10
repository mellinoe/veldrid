#if !EXCLUDE_METAL_BACKEND
using System.Collections.ObjectModel;
using System.Linq;
using Veldrid.MetalBindings;
using Veldrid.MTL;

namespace Veldrid
{
    /// <summary>
    /// Exposes Metal-specific functionality,
    /// useful for interoperating with native components which interface directly with Metal.
    /// Can only be used on <see cref="GraphicsBackend.Metal"/>.
    /// </summary>
    public class BackendInfoMetal
    {
        private readonly MTLGraphicsDevice _gd;
        private ReadOnlyCollection<MTLFeatureSet> _featureSet;

        internal BackendInfoMetal(MTLGraphicsDevice gd)
        {
            _gd = gd;
            _featureSet = new ReadOnlyCollection<MTLFeatureSet>(_gd.MetalFeatures.ToArray());
        }

        public ReadOnlyCollection<MTLFeatureSet> FeatureSet => _featureSet;

        public MTLFeatureSet MaxFeatureSet => _gd.MetalFeatures.MaxFeatureSet;
    }
}
#endif
