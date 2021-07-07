using System;
using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        /// <summary>
        /// A special sentinel value indicating that a newly-created window should be centered in the screen.
        /// </summary>
        public const int SDL_WINDOWPOS_CENTERED = 0x2FFF0000;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SDL_Window SDL_CreateWindow_t(byte* title, int x, int y, int w, int h, SDL_WindowFlags flags);
        private static SDL_CreateWindow_t s_sdl_createWindow = LoadFunction<SDL_CreateWindow_t>("SDL_CreateWindow");
        public static SDL_Window SDL_CreateWindow(string title, int x, int y, int w, int h, SDL_WindowFlags flags) => s_sdl_createWindow(Utf8EncodeNullable(title), x, y, w, h, flags);

        internal static unsafe byte* Utf8EncodeNullable(string str)
        {
            if (string.IsNullOrEmpty(str))
                return (byte*)0;

            var bytes = System.Text.Encoding.UTF8.GetBytes(str);
            var mem = System.Runtime.InteropServices.Marshal.AllocHGlobal(bytes.Length + 1);
            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, mem, bytes.Length);
            ((byte*)mem)[bytes.Length] = 0;
            return (byte*)mem;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SDL_Window SDL_CreateWindowFrom_t(IntPtr data);
        private static SDL_CreateWindowFrom_t s_sdl_createWindowFrom = LoadFunction<SDL_CreateWindowFrom_t>("SDL_CreateWindowFrom");
        public static SDL_Window SDL_CreateWindowFrom(IntPtr data) => s_sdl_createWindowFrom(data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_DestroyWindow_t(SDL_Window SDL2Window);
        private static SDL_DestroyWindow_t s_sdl_destroyWindow = LoadFunction<SDL_DestroyWindow_t>("SDL_DestroyWindow");
        public static void SDL_DestroyWindow(SDL_Window Sdl2Window) => s_sdl_destroyWindow(Sdl2Window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_GetWindowSize_t(SDL_Window SDL2Window, int* w, int* h);
        private static SDL_GetWindowSize_t s_getWindowSize = LoadFunction<SDL_GetWindowSize_t>("SDL_GetWindowSize");
        public static void SDL_GetWindowSize(SDL_Window Sdl2Window, int* w, int* h) => s_getWindowSize(Sdl2Window, w, h);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_GetWindowPosition_t(SDL_Window SDL2Window, int* x, int* y);
        private static SDL_GetWindowPosition_t s_getWindowPosition = LoadFunction<SDL_GetWindowPosition_t>("SDL_GetWindowPosition");
        public static void SDL_GetWindowPosition(SDL_Window Sdl2Window, int* x, int* y) => s_getWindowPosition(Sdl2Window, x, y);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_SetWindowPosition_t(SDL_Window SDL2Window, int x, int y);
        private static SDL_SetWindowPosition_t s_setWindowPosition = LoadFunction<SDL_SetWindowPosition_t>("SDL_SetWindowPosition");
        public static void SDL_SetWindowPosition(SDL_Window Sdl2Window, int x, int y) => s_setWindowPosition(Sdl2Window, x, y);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_SetWindowSize_t(SDL_Window SDL2Window, int w, int h);
        private static SDL_SetWindowSize_t s_setWindowSize = LoadFunction<SDL_SetWindowSize_t>("SDL_SetWindowSize");
        public static void SDL_SetWindowSize(SDL_Window Sdl2Window, int w, int h) => s_setWindowSize(Sdl2Window, w, h);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate string SDL_GetWindowTitle_t(SDL_Window SDL2Window);
        private static SDL_GetWindowTitle_t s_getWindowTitle = LoadFunction<SDL_GetWindowTitle_t>("SDL_GetWindowTitle");
        public static string SDL_GetWindowTitle(SDL_Window Sdl2Window) => s_getWindowTitle(Sdl2Window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_SetWindowTitle_t(SDL_Window SDL2Window, string title);
        private static SDL_SetWindowTitle_t s_setWindowTitle = LoadFunction<SDL_SetWindowTitle_t>("SDL_SetWindowTitle");
        public static void SDL_SetWindowTitle(SDL_Window Sdl2Window, string title) => s_setWindowTitle(Sdl2Window, title);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SDL_WindowFlags SDL_GetWindowFlags_t(SDL_Window SDL2Window);
        private static SDL_GetWindowFlags_t s_getWindowFlags = LoadFunction<SDL_GetWindowFlags_t>("SDL_GetWindowFlags");
        public static SDL_WindowFlags SDL_GetWindowFlags(SDL_Window Sdl2Window) => s_getWindowFlags(Sdl2Window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_SetWindowBordered_t(SDL_Window SDL2Window, uint bordered);
        private static SDL_SetWindowBordered_t s_setWindowBordered = LoadFunction<SDL_SetWindowBordered_t>("SDL_SetWindowBordered");
        public static void SDL_SetWindowBordered(SDL_Window Sdl2Window, uint bordered) => s_setWindowBordered(Sdl2Window, bordered);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_MaximizeWindow_t(SDL_Window SDL2Window);
        private static SDL_MaximizeWindow_t s_maximizeWindow = LoadFunction<SDL_MaximizeWindow_t>("SDL_MaximizeWindow");
        public static void SDL_MaximizeWindow(SDL_Window Sdl2Window) => s_maximizeWindow(Sdl2Window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_MinimizeWindow_t(SDL_Window SDL2Window);
        private static SDL_MinimizeWindow_t s_minimizeWindow = LoadFunction<SDL_MinimizeWindow_t>("SDL_MinimizeWindow");
        public static void SDL_MinimizeWindow(SDL_Window Sdl2Window) => s_minimizeWindow(Sdl2Window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_RaiseWindow_t(SDL_Window SDL2Window);
        private static SDL_RaiseWindow_t s_raiseWindow = LoadFunction<SDL_RaiseWindow_t>("SDL_RaiseWindow");
        public static void SDL_RaiseWindow(SDL_Window Sdl2Window) => s_raiseWindow(Sdl2Window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_SetWindowFullscreen_t(SDL_Window Sdl2Window, SDL_FullscreenMode mode);
        private static SDL_SetWindowFullscreen_t s_setWindowFullscreen = LoadFunction<SDL_SetWindowFullscreen_t>("SDL_SetWindowFullscreen");
        public static int SDL_SetWindowFullscreen(SDL_Window Sdl2Window, SDL_FullscreenMode mode) => s_setWindowFullscreen(Sdl2Window, mode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_ShowWindow_t(SDL_Window SDL2Window);
        private static SDL_ShowWindow_t s_showWindow = LoadFunction<SDL_ShowWindow_t>("SDL_ShowWindow");
        public static void SDL_ShowWindow(SDL_Window Sdl2Window) => s_showWindow(Sdl2Window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_HideWindow_t(SDL_Window SDL2Window);
        private static SDL_HideWindow_t s_hideWindow = LoadFunction<SDL_HideWindow_t>("SDL_HideWindow");
        public static void SDL_HideWindow(SDL_Window Sdl2Window) => s_hideWindow(Sdl2Window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint SDL_GetWindowID_t(SDL_Window SDL2Window);
        private static SDL_GetWindowID_t s_getWindowID = LoadFunction<SDL_GetWindowID_t>("SDL_GetWindowID");
        public static uint SDL_GetWindowID(SDL_Window Sdl2Window) => s_getWindowID(Sdl2Window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_SetWindowOpacity_t(SDL_Window window, float opacity);
        private static SDL_SetWindowOpacity_t s_setWindowOpacity = LoadFunction<SDL_SetWindowOpacity_t>("SDL_SetWindowOpacity");
        public static int SDL_SetWindowOpacity(SDL_Window Sdl2Window, float opacity) => s_setWindowOpacity(Sdl2Window, opacity);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GetWindowOpacity_t(SDL_Window window, float* opacity);
        private static SDL_GetWindowOpacity_t s_getWindowOpacity = LoadFunction<SDL_GetWindowOpacity_t>("SDL_GetWindowOpacity");
        public static int SDL_GetWindowOpacity(SDL_Window Sdl2Window, float* opacity) => s_getWindowOpacity(Sdl2Window, opacity);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_SetWindowResizable_t(SDL_Window window, uint resizable);
        private static SDL_SetWindowResizable_t s_setWindowResizable = LoadFunction<SDL_SetWindowResizable_t>("SDL_SetWindowResizable");
        public static void SDL_SetWindowResizable(SDL_Window window, uint resizable) => s_setWindowResizable(window, resizable);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GetDisplayBounds_t(int displayIndex, Rectangle* rect);
        private static SDL_GetDisplayBounds_t s_sdl_getDisplayBounds = LoadFunction<SDL_GetDisplayBounds_t>("SDL_GetDisplayBounds");
        public static int SDL_GetDisplayBounds(int displayIndex, Rectangle* rect) => s_sdl_getDisplayBounds(displayIndex, rect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GetWindowDisplayIndex_t(SDL_Window window);
        private static SDL_GetWindowDisplayIndex_t s_sdl_getWindowDisplayIndex = LoadFunction<SDL_GetWindowDisplayIndex_t>("SDL_GetWindowDisplayIndex");
        public static int SDL_GetWindowDisplayIndex(SDL_Window window) => s_sdl_getWindowDisplayIndex(window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GetCurrentDisplayMode_t(int displayIndex, SDL_DisplayMode* mode);
        private static SDL_GetCurrentDisplayMode_t s_sdl_getCurrentDisplayMode = LoadFunction<SDL_GetCurrentDisplayMode_t>("SDL_GetCurrentDisplayMode");
        public static int SDL_GetCurrentDisplayMode(int displayIndex, SDL_DisplayMode* mode) => s_sdl_getCurrentDisplayMode(displayIndex, mode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GetDesktopDisplayMode_t(int displayIndex, SDL_DisplayMode* mode);
        private static SDL_GetDesktopDisplayMode_t s_sdl_getDesktopDisplayMode = LoadFunction<SDL_GetDesktopDisplayMode_t>("SDL_GetDesktopDisplayMode");
        public static int SDL_GetDesktopDisplayMode(int displayIndex, SDL_DisplayMode* mode) => s_sdl_getDesktopDisplayMode(displayIndex, mode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GetNumVideoDisplays_t();
        private static SDL_GetNumVideoDisplays_t s_sdl_getNumVideoDisplays = LoadFunction<SDL_GetNumVideoDisplays_t>("SDL_GetNumVideoDisplays");
        public static int SDL_GetNumVideoDisplays() => s_sdl_getNumVideoDisplays();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate bool SDL_SetHint_t(string name, string value);
        private static SDL_SetHint_t s_sdl_setHint = LoadFunction<SDL_SetHint_t>("SDL_SetHint");
        public static bool SDL_SetHint(string name, string value) => s_sdl_setHint(name, value);
        
    }

    [Flags]
    public enum SDL_WindowFlags : uint
    {
        /// <summary>
        /// fullscreen Sdl2Window.
        /// </summary>
        Fullscreen = 0x00000001,
        /// <summary>
        /// Sdl2Window usable with OpenGL context.
        /// </summary>
        OpenGL = 0x00000002,
        /// <summary>
        /// Sdl2Window is visible.
        /// </summary>
        Shown = 0x00000004,
        /// <summary>
        /// Sdl2Window is not visible.
        /// </summary>
        Hidden = 0x00000008,
        /// <summary>
        /// no Sdl2Window decoration.
        /// </summary>
        Borderless = 0x00000010,
        /// <summary>
        /// Sdl2Window can be resized.
        /// </summary>
        Resizable = 0x00000020,
        /// <summary>
        /// Sdl2Window is minimized.
        /// </summary>
        Minimized = 0x00000040,
        /// <summary>
        /// Sdl2Window is maximized.
        /// </summary>
        Maximized = 0x00000080,
        /// <summary>
        /// Sdl2Window has grabbed input focus.
        /// </summary>
        InputGrabbed = 0x00000100,
        /// <summary>
        /// Sdl2Window has input focus.
        /// </summary>
        InputFocus = 0x00000200,
        /// <summary>
        /// Sdl2Window has mouse focus.
        /// </summary>
        MouseFocus = 0x00000400,
        FullScreenDesktop = (Fullscreen | 0x00001000),
        /// <summary>
        /// Sdl2Window not created by SDL.
        /// </summary>
        Foreign = 0x00000800,
        /// <summary>
        /// Sdl2Window should be created in high-DPI mode if supported.
        /// </summary>
        AllowHighDpi = 0x00002000,
        /// <summary>
        /// Sdl2Window has mouse captured (unrelated to InputGrabbed).
        /// </summary>
        MouseCapture = 0x00004000,
        /// <summary>
        /// Sdl2Window should always be above others.
        /// </summary>
        AlwaysOnTop = 0x00008000,
        /// <summary>
        /// Sdl2Window should not be added to the taskbar.
        /// </summary>
        SkipTaskbar = 0x00010000,
        /// <summary>
        /// Sdl2Window should be treated as a utility Sdl2Window.
        /// </summary>
        Utility = 0x00020000,
        /// <summary>
        /// Sdl2Window should be treated as a tooltip.
        /// </summary>
        Tooltip = 0x00040000,
        /// <summary>
        /// Sdl2Window should be treated as a popup menu.
        /// </summary>
        PopupMenu = 0x00080000
    }

    public enum SDL_FullscreenMode : uint
    {
        Windowed = 0,
        Fullscreen = 0x00000001,
        FullScreenDesktop = (Fullscreen | 0x00001000),
    }

    public unsafe struct SDL_DisplayMode
    {
        public uint format;
        public int w;
        public int h;
        public int refresh_rate;
        public void* driverdata;
    }
}
