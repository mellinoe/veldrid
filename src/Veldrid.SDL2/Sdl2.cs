using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        private static readonly IntPtr s_sdl2Lib = LoadSdl2();

        private static IntPtr LoadSdl2()
        {
            string name;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                name = "SDL2.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                name = "libSDL2-2.0.so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                name = "libsdl2.dylib";
            }
            else
            {
                Debug.WriteLine("Unknown SDL platform. Attempting to load \"SDL2\"");
                name = "SDL2.dll";
            }

            return NativeLibrary.Load(
                name,
                Assembly.GetExecutingAssembly(),
                DllImportSearchPath.SafeDirectories);
        }

        /// <summary>
        /// Loads an SDL2 function by the given name.
        /// </summary>
        /// <typeparam name="T">The delegate type of the function to load.</typeparam>
        /// <param name="name">The name of the exported native function.</param>
        /// <returns>A delegate which can be used to invoke the native function.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when no function with the given name is exported by SDL2.
        /// </exception>
        public static T LoadFunction<T>(string name)
        {
            try
            {
                IntPtr export = NativeLibrary.GetExport(s_sdl2Lib, name);
                return Marshal.GetDelegateForFunctionPointer<T>(export);
            }
            catch
            {
                Debug.WriteLine(
                    $"Unable to load SDL2 function \"{name}\". " +
                    $"Attempting to call this function will cause an exception to be thrown.");
                return default(T);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* SDL_GetError_t();
        private static SDL_GetError_t s_sdl_getError = LoadFunction<SDL_GetError_t>("SDL_GetError");
        public static byte* SDL_GetError() => s_sdl_getError();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_ClearError_t();
        private static SDL_ClearError_t s_sdl_clearError = LoadFunction<SDL_ClearError_t>("SDL_ClearError");
        public static byte* SDL_ClearError() { s_sdl_clearError(); return null; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_free_t(void* ptr);
        private static SDL_free_t s_sdl_free = LoadFunction<SDL_free_t>("SDL_free");
        public static void SDL_free(void* ptr) { s_sdl_free(ptr); }
    }
}
