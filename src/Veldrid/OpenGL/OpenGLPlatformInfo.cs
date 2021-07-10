using System;

namespace Veldrid.OpenGL
{
    /// <summary>
    /// Encapsulates various pieces of OpenGL context, necessary for creating a <see cref="GraphicsDevice"/> using the OpenGL
    /// API.
    /// </summary>
    public class OpenGLPlatformInfo
    {
        /// <summary>
        /// The OpenGL context handle.
        /// </summary>
        public IntPtr OpenGLContextHandle { get; }

        /// <summary>
        /// A delegate which can be used to retrieve OpenGL function pointers by name.
        /// </summary>
        public Func<string, IntPtr> GetProcAddress { get; }

        /// <summary>
        /// A delegate which can be used to make the given OpenGL context current on the calling thread.
        /// </summary>
        public Action<IntPtr> MakeCurrent { get; }

        /// <summary>
        /// A delegate which can be used to retrieve the calling thread's active OpenGL context.
        /// </summary>
        public Func<IntPtr> GetCurrentContext { get; }

        /// <summary>
        /// A delegate which can be used to clear the calling thread's GL context.
        /// </summary>
        public Action ClearCurrentContext { get; }

        /// <summary>
        /// A delegate which can be used to delete the given context.
        /// </summary>
        public Action<IntPtr> DeleteContext { get; }

        /// <summary>
        /// A delegate which can be used to swap the main back buffer associated with the OpenGL context.
        /// </summary>
        public Action SwapBuffers { get; }

        /// <summary>
        /// A delegate which can be used to set the synchronization behavior of the OpenGL context.
        /// </summary>
        public Action<bool> SetSyncToVerticalBlank { get; }

        /// <summary>
        /// A delegate which can be used to set the framebuffer used to render to the application Swapchain.
        /// If this is null, the default FBO (0) will be bound.
        /// </summary>
        public Action SetSwapchainFramebuffer { get; }

        /// <summary>
        /// A delegate which is invoked when the main Swapchain is resized. This may be null, in which case
        /// no special action is taken when the Swapchain is resized.
        /// </summary>
        public Action<uint, uint> ResizeSwapchain { get; }

        /// <summary>
        /// Constructs a new OpenGLPlatformInfo.
        /// </summary>
        /// <param name="openGLContextHandle">The OpenGL context handle.</param>
        /// <param name="getProcAddress">A delegate which can be used to retrieve OpenGL function pointers by name.</param>
        /// <param name="makeCurrent">A delegate which can be used to make the given OpenGL context current on the calling
        /// thread.</param>
        /// <param name="getCurrentContext">A delegate which can be used to retrieve the calling thread's active OpenGL context.</param>
        /// <param name="clearCurrentContext">A delegate which can be used to clear the calling thread's GL context.</param>
        /// <param name="deleteContext">A delegate which can be used to delete the given context.</param>
        /// <param name="swapBuffers">A delegate which can be used to swap the main back buffer associated with the OpenGL
        /// context.</param>
        /// <param name="setSyncToVerticalBlank">A delegate which can be used to set the synchronization behavior of the OpenGL
        /// context.</param>
        public OpenGLPlatformInfo(
            IntPtr openGLContextHandle,
            Func<string, IntPtr> getProcAddress,
            Action<IntPtr> makeCurrent,
            Func<IntPtr> getCurrentContext,
            Action clearCurrentContext,
            Action<IntPtr> deleteContext,
            Action swapBuffers,
            Action<bool> setSyncToVerticalBlank)
        {
            OpenGLContextHandle = openGLContextHandle;
            GetProcAddress = getProcAddress;
            MakeCurrent = makeCurrent;
            GetCurrentContext = getCurrentContext;
            ClearCurrentContext = clearCurrentContext;
            DeleteContext = deleteContext;
            SwapBuffers = swapBuffers;
            SetSyncToVerticalBlank = setSyncToVerticalBlank;
        }

        /// <summary>
        /// Constructs a new OpenGLPlatformInfo.
        /// </summary>
        /// <param name="openGLContextHandle">The OpenGL context handle.</param>
        /// <param name="getProcAddress">A delegate which can be used to retrieve OpenGL function pointers by name.</param>
        /// <param name="makeCurrent">A delegate which can be used to make the given OpenGL context current on the calling
        /// thread.</param>
        /// <param name="getCurrentContext">A delegate which can be used to retrieve the calling thread's active OpenGL context.</param>
        /// <param name="clearCurrentContext">A delegate which can be used to clear the calling thread's GL context.</param>
        /// <param name="deleteContext">A delegate which can be used to delete the given context.</param>
        /// <param name="swapBuffers">A delegate which can be used to swap the main back buffer associated with the OpenGL
        /// context.</param>
        /// <param name="setSyncToVerticalBlank">A delegate which can be used to set the synchronization behavior of the OpenGL
        /// context.</param>
        /// <param name="setSwapchainFramebuffer">A delegate which can be used to set the framebuffer used to render to the
        /// application Swapchain.</param>
        /// <param name="resizeSwapchain">A delegate which is invoked when the main Swapchain is resized. This may be null,
        /// in which case no special action is taken when the Swapchain is resized.</param>
        public OpenGLPlatformInfo(
            IntPtr openGLContextHandle,
            Func<string, IntPtr> getProcAddress,
            Action<IntPtr> makeCurrent,
            Func<IntPtr> getCurrentContext,
            Action clearCurrentContext,
            Action<IntPtr> deleteContext,
            Action swapBuffers,
            Action<bool> setSyncToVerticalBlank,
            Action setSwapchainFramebuffer,
            Action<uint, uint> resizeSwapchain)
        {
            OpenGLContextHandle = openGLContextHandle;
            GetProcAddress = getProcAddress;
            MakeCurrent = makeCurrent;
            GetCurrentContext = getCurrentContext;
            ClearCurrentContext = clearCurrentContext;
            DeleteContext = deleteContext;
            SwapBuffers = swapBuffers;
            SetSyncToVerticalBlank = setSyncToVerticalBlank;
            SetSwapchainFramebuffer = setSwapchainFramebuffer;
            ResizeSwapchain = resizeSwapchain;
        }
    }
}
