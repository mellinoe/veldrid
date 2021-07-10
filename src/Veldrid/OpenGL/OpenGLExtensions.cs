using System.Collections.Generic;

namespace Veldrid.OpenGL
{
    internal class OpenGLExtensions : IReadOnlyCollection<string>
    {
        private readonly HashSet<string> _extensions;
        private readonly GraphicsBackend _backend;
        private readonly int _major;
        private readonly int _minor;

        public int Count => _extensions.Count;

        internal OpenGLExtensions(HashSet<string> extensions, GraphicsBackend backend, int major, int minor)
        {
            _extensions = extensions;
            _backend = backend;
            _major = major;
            _minor = minor;

            TextureStorage = IsExtensionSupported("GL_ARB_texture_storage") // OpenGL 4.2 / 4.3 (multisampled)
                || GLESVersion(3, 0);
            TextureStorageMultisample = IsExtensionSupported("GL_ARB_texture_storage_multisample")
                || GLESVersion(3, 1);
            ARB_DirectStateAccess = GLVersion(4, 5) || IsExtensionSupported("GL_ARB_direct_state_access");
            ARB_MultiBind = IsExtensionSupported("GL_ARB_multi_bind");
            ARB_TextureView = GLVersion(4, 3) || IsExtensionSupported("GL_ARB_texture_view"); // OpenGL 4.3
            CopyImage = IsExtensionSupported("GL_ARB_copy_image")
                || GLESVersion(3, 2)
                || IsExtensionSupported("GL_OES_copy_image")
                || IsExtensionSupported("GL_EXT_copy_image");
            ARB_DebugOutput = IsExtensionSupported("GL_ARB_debug_output");
            KHR_Debug = IsExtensionSupported("GL_KHR_debug");

            ComputeShaders = IsExtensionSupported("GL_ARB_compute_shader") || GLESVersion(3, 1);

            ARB_ViewportArray = IsExtensionSupported("GL_ARB_viewport_array") || GLVersion(4, 1);
            TessellationShader = IsExtensionSupported("GL_ARB_tessellation_shader") || GLVersion(4, 0)
                || IsExtensionSupported("GL_OES_tessellation_shader");
            GeometryShader = IsExtensionSupported("GL_ARB_geometry_shader4") || GLVersion(3, 2)
                || IsExtensionSupported("OES_geometry_shader");
            DrawElementsBaseVertex = GLVersion(3, 2)
                || IsExtensionSupported("GL_ARB_draw_elements_base_vertex")
                || GLESVersion(3, 2)
                || IsExtensionSupported("GL_OES_draw_elements_base_vertex");
            IndependentBlend = GLVersion(4, 0) || IsExtensionSupported("GL_ARB_draw_buffers_blend") || GLESVersion(3, 2);

            DrawIndirect = GLVersion(4, 0) || IsExtensionSupported("GL_ARB_draw_indirect")
                || GLESVersion(3, 1);
            MultiDrawIndirect = GLVersion(4, 3) || IsExtensionSupported("GL_ARB_multi_draw_indirect")
                || IsExtensionSupported("GL_EXT_multi_draw_indirect");

            StorageBuffers = GLVersion(4, 3) || IsExtensionSupported("GL_ARB_shader_storage_buffer_object")
                || GLESVersion(3, 1);

            ARB_ClipControl = GLVersion(4, 5) || IsExtensionSupported("GL_ARB_clip_control");
            EXT_sRGBWriteControl = _backend == GraphicsBackend.OpenGLES && IsExtensionSupported("GL_EXT_sRGB_write_control");
            EXT_DebugMarker = _backend == GraphicsBackend.OpenGLES && IsExtensionSupported("GL_EXT_debug_marker");

            ARB_GpuShaderFp64 = GLVersion(4, 0) || IsExtensionSupported("GL_ARB_gpu_shader_fp64");

            ARB_uniform_buffer_object = IsExtensionSupported("GL_ARB_uniform_buffer_object");

            AnisotropicFilter = IsExtensionSupported("GL_EXT_texture_filter_anisotropic") || IsExtensionSupported("GL_ARB_texture_filter_anisotropic");
        }

        public readonly bool ARB_DirectStateAccess;
        public readonly bool ARB_MultiBind;
        public readonly bool ARB_TextureView;
        public readonly bool ARB_DebugOutput;
        public readonly bool KHR_Debug;
        public readonly bool ARB_ViewportArray;
        public readonly bool ARB_ClipControl;
        public readonly bool EXT_sRGBWriteControl;
        public readonly bool EXT_DebugMarker;
        public readonly bool ARB_GpuShaderFp64;
        public readonly bool ARB_uniform_buffer_object;

        // Differs between GL / GLES
        public readonly bool TextureStorage;
        public readonly bool TextureStorageMultisample;

        public readonly bool CopyImage;
        public readonly bool ComputeShaders;
        public readonly bool TessellationShader;
        public readonly bool GeometryShader;
        public readonly bool DrawElementsBaseVertex;
        public readonly bool IndependentBlend;
        public readonly bool DrawIndirect;
        public readonly bool MultiDrawIndirect;
        public readonly bool StorageBuffers;
        public readonly bool AnisotropicFilter;

        /// <summary>
        /// Returns a value indicating whether the given extension is supported.
        /// </summary>
        /// <param name="extension">The name of the extensions. For example, "</param>
        /// <returns></returns>
        public bool IsExtensionSupported(string extension)
        {
            return _extensions.Contains(extension);
        }

        public bool GLVersion(int major, int minor)
        {
            if (_backend == GraphicsBackend.OpenGL)
            {
                if (_major > major)
                {
                    return true;
                }
                else
                {
                    return _major == major && _minor >= minor;
                }
            }

            return false;
        }

        public bool GLESVersion(int major, int minor)
        {
            if (_backend == GraphicsBackend.OpenGLES)
            {
                if (_major > major)
                {
                    return true;
                }
                else
                {
                    return _major == major && _minor >= minor;
                }
            }

            return false;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _extensions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
