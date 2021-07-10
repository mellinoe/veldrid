using NativeLibraryLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using NativeLibrary = NativeLibraryLoader.NativeLibrary;

namespace Veldrid
{
    /// <summary>
    /// Provides access to RenderDoc's in-application API. Can be used to configure, collect, and save RenderDoc capture files,
    /// and to launch and manage the RenderDoc replay UI application.
    /// </summary>
    public unsafe class RenderDoc
    {
        private readonly RENDERDOC_API_1_4_0 _api;
        private readonly NativeLibrary _nativeLib;

        private unsafe RenderDoc(NativeLibrary lib)
        {
            _nativeLib = lib;
            pRENDERDOC_GetAPI getApiFunc = _nativeLib.LoadFunction<pRENDERDOC_GetAPI>("RENDERDOC_GetAPI");
            void* apiPointers;
            int result = getApiFunc(RENDERDOC_Version.API_Version_1_2_0, &apiPointers);
            if (result != 1)
            {
                throw new InvalidOperationException("Failed to load RenderDoc API.");
            }

            _api = Marshal.PtrToStructure<RENDERDOC_API_1_4_0>((IntPtr)apiPointers);
        }

        /// <summary>
        /// Allow the application to enable vsync.
        /// Default value: true.
        /// true: The application can enable or disable vsync at will.
        /// false: vsync is force disabled.
        /// </summary>
        public bool AllowVSync
        {
            get => _api.GetCaptureOptionU32(RENDERDOC_CaptureOption.AllowVSync) != 0;
            set => _api.SetCaptureOptionU32(RENDERDOC_CaptureOption.AllowVSync, value ? 1u : 0u);
        }

        /// <summary>
        /// Allow the application to enable fullscreen.
        /// Default value: true.
        /// true: The application can enable or disable fullscreen at will.
        /// false: fullscreen is force disabled.
        /// </summary>
        public bool AllowFullscreen
        {
            get => _api.GetCaptureOptionU32(RENDERDOC_CaptureOption.AllowFullscreen) != 0;
            set => _api.SetCaptureOptionU32(RENDERDOC_CaptureOption.AllowFullscreen, value ? 1u : 0u);
        }

        /// <summary>
        /// Record API debugging events and messages.
        /// Default value: false.
        /// true: Enable built-in API debugging features and records the results into the capture, which is matched up with
        /// events on replay.
        /// false: no API debugging is forcibly enabled.
        /// </summary>
        public bool APIValidation
        {
            get => _api.GetCaptureOptionU32(RENDERDOC_CaptureOption.APIValidation) != 0;
            set => _api.SetCaptureOptionU32(RENDERDOC_CaptureOption.APIValidation, value ? 1u : 0u);
        }

        /// <summary>
        /// Capture CPU callstacks for API events.
        /// Default value: false.
        /// true: Enables capturing of callstacks.
        /// false: no callstacks are captured.
        /// </summary>
        public bool CaptureCallstacks
        {
            get => _api.GetCaptureOptionU32(RENDERDOC_CaptureOption.CaptureCallstacks) != 0;
            set => _api.SetCaptureOptionU32(RENDERDOC_CaptureOption.CaptureCallstacks, value ? 1u : 0u);
        }

        /// <summary>
        /// When capturing CPU callstacks, only capture them from drawcalls.
        /// This option does nothing without CaptureCallstacks being enabled.
        /// Default value: false.
        /// true: Only captures callstacks for drawcall type API events. Ignored if CaptureCallstacks is disabled.
        /// false: Callstacks, if enabled, are captured for every event.
        /// </summary>
        public bool CaptureCallstacksOnlyDraws
        {
            get => _api.GetCaptureOptionU32(RENDERDOC_CaptureOption.CaptureCallstacksOnlyDraws) != 0;
            set => _api.SetCaptureOptionU32(RENDERDOC_CaptureOption.CaptureCallstacksOnlyDraws, value ? 1u : 0u);
        }

        /// <summary>
        /// Specify a delay in seconds to wait for a debugger to attach, after creating or injecting into a process, before
        /// continuing to allow it to run.
        /// A value of 0 indicates no delay, and the process will run immediately after injection.
        /// Default value: 0 seconds.
        /// </summary>
        public uint DelayForDebugger
        {
            get => _api.GetCaptureOptionU32(RENDERDOC_CaptureOption.DelayForDebugger);
            set => _api.SetCaptureOptionU32(RENDERDOC_CaptureOption.DelayForDebugger, value);
        }

        /// <summary>
        /// Verify buffer access. This includes checking the memory returned by a Map() call to detect any out-of-bounds
        /// modification, as well as initialising buffers with undefined contents to a marker value to catch use of uninitialised
        /// memory.
        /// NOTE: This option is only valid for OpenGL and D3D11. Explicit APIs such as D3D12 and Vulkan do
        /// not do the same kind of interception & checking and undefined contents are really undefined.
        /// Default value: false.
        /// true: Verify buffer access.
        /// false: No verification is performed, and overwriting bounds may cause crashes or corruption in RenderDoc.
        /// </summary>
        public bool VerifyBufferAccess
        {
            get => _api.GetCaptureOptionU32(RENDERDOC_CaptureOption.VerifyBufferAccess) != 0;
            set => _api.SetCaptureOptionU32(RENDERDOC_CaptureOption.VerifyBufferAccess, value ? 1u : 0u);
        }

        /// <summary>
        /// Hooks any system API calls that create child processes, and injects RenderDoc into them recursively with the same
        /// options.
        /// Default value: false.
        /// true: Hooks into spawned child processes.
        /// false: Child processes are not hooked by RenderDoc.
        /// </summary>
        public bool HookIntoChildren
        {
            get => _api.GetCaptureOptionU32(RENDERDOC_CaptureOption.HookIntoChildren) != 0;
            set => _api.SetCaptureOptionU32(RENDERDOC_CaptureOption.HookIntoChildren, value ? 1u : 0u);
        }

        /// <summary>
        /// By default RenderDoc only includes resources in the final capture necessary for that frame, this allows you to
        /// override that behaviour.
        /// Default value: false.
        /// true: all live resources at the time of capture are included in the capture and available for inspection.
        /// false: only the resources referenced by the captured frame are included.
        /// </summary>
        public bool RefAllResources
        {
            get => _api.GetCaptureOptionU32(RENDERDOC_CaptureOption.RefAllResources) != 0;
            set => _api.SetCaptureOptionU32(RENDERDOC_CaptureOption.RefAllResources, value ? 1u : 0u);
        }

        /// <summary>
        /// In APIs that allow for the recording of command lists to be replayed later, RenderDoc may choose to not capture
        /// command lists before a frame capture is triggered, to reduce overheads. This means any command lists recorded once
        /// and replayed many times will not be available and may cause a failure to capture.
        /// NOTE: This is only true for APIs where multithreading is difficult or discouraged. Newer APIs like Vulkan and D3D12
        /// will ignore this option and always capture all command lists since the API is heavily oriented around it and the
        /// overheads have been reduced by API design.
        /// true: All command lists are captured from the start of the application.
        /// false: Command lists are only captured if their recording begins during the period when a frame capture is in
        /// progress.
        /// </summary>
        public bool CaptureAllCmdLists
        {
            get => _api.GetCaptureOptionU32(RENDERDOC_CaptureOption.CaptureAllCmdLists) != 0;
            set => _api.SetCaptureOptionU32(RENDERDOC_CaptureOption.CaptureAllCmdLists, value ? 1u : 0u);
        }

        /// <summary>
        /// Mute API debugging output when the API validation mode option is enabled
        /// Default value: true.
        /// true: Mute any API debug messages from being displayed or passed through.
        /// false: API debugging is displayed as normal.
        /// </summary>
        public bool DebugOutputMute
        {
            get => _api.GetCaptureOptionU32(RENDERDOC_CaptureOption.DebugOutputMute) != 0;
            set => _api.SetCaptureOptionU32(RENDERDOC_CaptureOption.DebugOutputMute, value ? 1u : 0u);
        }

        /// <summary>
        /// Capture the next frame on whichever window and API is currently considered active.
        /// </summary>
        public void TriggerCapture() => _api.TriggerCapture();

        /// <summary>
        /// Capture the next N frames on whichever window and API is currently considered active.
        /// </summary>
        /// <param name="numFrames">The number of frames to capture.</param>
        public void TriggerCapture(uint numFrames) => _api.TriggerMultiFrameCapture(numFrames);

        /// <summary>
        /// Immediately starts capturing API calls on the active device and window.
        /// If there is no matching thing to capture (e.g. no supported API has been initialised), this will do nothing.
        /// The results are undefined (including crashes) if two captures are started overlapping, even on separate devices
        /// and/or windows.
        /// </summary>
        public void StartFrameCapture() => _api.StartFrameCapture(null, null);

        /// <summary>
        /// Immediately starts capturing API calls on the active device and window.
        /// If there is no matching thing to capture (e.g. no supported API has been initialised), this will do nothing.
        /// The results are undefined (including crashes) if two captures are started overlapping, even on separate devices
        /// and/or windows.
        /// </summary>
        /// <param name="device"> is a handle to the API ‘device’ object that will be set active. May be null for wildcard match.</param>
        /// <param name="wndHandle"> is a handle to the platform window handle that will be set active. May be null for wildcard match.</param>
        public void StartFrameCapture(IntPtr device, IntPtr wndHandle) => _api.StartFrameCapture(device.ToPointer(), wndHandle.ToPointer());


        /// <summary>
        /// Returns whether or not a frame capture is currently ongoing anywhere.
        /// </summary>
        /// <returns>True if a capture is ongoing, false if there is no capture running.</returns>
        public bool IsFrameCapturing() => _api.IsFrameCapturing() != 0;

        /// <summary>
        /// Ends capturing immediately.
        /// </summary>
        /// <returns>True if the capture succeeded, false if there was an error capturing.</returns>
        public bool EndFrameCapture() => _api.EndFrameCapture(null, null) != 0;

        /// <summary>
        /// Ends capturing immediately.
        /// </summary>
        /// <param name="device"> is a handle to the API ‘device’ object that will be set active. May be null for wildcard match.</param>
        /// <param name="wndHandle"> is a handle to the platform window handle that will be set active. May be null for wildcard match.</param>
        /// <returns>True if the capture succeeded, false if there was an error capturing.</returns>
        public bool EndFrameCapture(IntPtr device, IntPtr wndHandle) => _api.EndFrameCapture(device.ToPointer(), wndHandle.ToPointer()) != 0;

        /// <summary>
        /// This function will launch the Replay UI associated with the RenderDoc library injected into the running application.
        /// </summary>
        /// <returns>The PID of the replay UI if successful, 0 if not successful.</returns>
        public uint LaunchReplayUI() => _api.LaunchReplayUI(1, null);

        /// <summary>
        /// This function will explicitly set which window is considered active.
        ///  The active window is the one that will be captured when the keybind to trigger a capture is pressed.
        /// </summary>
        /// <param name="device"> is a handle to the API ‘device’ object that will be set active. Must be valid.</param>
        /// <param name="wndHandle"> is a handle to the platform window handle that will be set active. Must be valid.</param>
        public void SetActiveWindow(IntPtr device, IntPtr wndHandle) => _api.SetActiveWindow(device.ToPointer(), wndHandle.ToPointer());

        /// <summary>
        /// This function will launch the Replay UI associated with the RenderDoc library injected into the running application.
        /// </summary>
        /// <param name="args">The rest of the command line, e.g. a capture file to open</param>
        /// <returns>The PID of the replay UI if successful, 0 if not successful.</returns>
        public uint LaunchReplayUI(string args)
        {
            int asciiByteCount = Encoding.ASCII.GetByteCount(args);
            byte* asciiBytes = stackalloc byte[asciiByteCount + 1];
            fixed (char* argsPtr = args)
            {
                int encoded = Encoding.ASCII.GetBytes(argsPtr, args.Length, asciiBytes, asciiByteCount);
                asciiBytes[encoded] = 0;
            }

            return _api.LaunchReplayUI(1, asciiBytes);
        }

        /// <summary>
        /// Gets the number of captures that have been made.
        /// </summary>
        public uint CaptureCount => _api.GetNumCaptures();

        /// <summary>
        /// Sets the path into which capture files will be saved.
        /// </summary>
        /// <param name="path">The path to save capture files under.</param>
        public void SetCaptureSavePath(string path)
        {
            int asciiByteCount = Encoding.ASCII.GetByteCount(path);
            byte* asciiBytes = stackalloc byte[asciiByteCount + 1];
            fixed (char* argsPtr = path)
            {
                int encoded = Encoding.ASCII.GetBytes(argsPtr, path.Length, asciiBytes, asciiByteCount);
                asciiBytes[encoded] = 0;
            }

            _api.SetCaptureFilePathTemplate(asciiBytes);
        }

        /// <summary>
        /// Gets a value indicating whether the RenderDoc UI is connected to this application.
        /// </summary>
        /// <returns>true if the RenderDoc UI is connected to this application, false otherwise.</returns>
        public bool IsTargetControlConnected() => _api.IsTargetControlConnected() == 1;

        /// <summary>
        /// Controls whether the overlay is enabled or disabled globally.
        /// </summary>
        public bool OverlayEnabled
        {
            get => (_api.GetOverlayBits() & (uint)RENDERDOC_OverlayBits.Enabled) != 0;
            set
            {
                uint bit = (uint)RENDERDOC_OverlayBits.Enabled;
                if (value) { _api.MaskOverlayBits(~0u, bit); }
                else { _api.MaskOverlayBits(~bit, 0); }
            }
        }

        /// <summary>
        /// Controls whether the overlay displays the average framerate over several seconds as well as min/max.
        /// </summary>
        public bool OverlayFrameRate
        {
            get => (_api.GetOverlayBits() & (uint)RENDERDOC_OverlayBits.FrameRate) != 0;
            set
            {
                uint bit = (uint)RENDERDOC_OverlayBits.FrameRate;
                if (value) { _api.MaskOverlayBits(~0u, bit); }
                else { _api.MaskOverlayBits(~bit, 0); }
            }
        }

        /// <summary>
        /// Controls whether the overlay displays the current frame number.
        /// </summary>
        public bool OverlayFrameNumber
        {
            get => (_api.GetOverlayBits() & (uint)RENDERDOC_OverlayBits.FrameNumber) != 0;
            set
            {
                uint bit = (uint)RENDERDOC_OverlayBits.FrameNumber;
                if (value) { _api.MaskOverlayBits(~0u, bit); }
                else { _api.MaskOverlayBits(~bit, 0); }
            }
        }

        /// <summary>
        /// Controls whether the overlay displays a list of recent captures, and how many captures have been made.
        /// </summary>
        public bool OverlayCaptureList
        {
            get => (_api.GetOverlayBits() & (uint)RENDERDOC_OverlayBits.CaptureList) != 0;
            set
            {
                uint bit = (uint)RENDERDOC_OverlayBits.CaptureList;
                if (value) { _api.MaskOverlayBits(~0u, bit); }
                else { _api.MaskOverlayBits(~bit, 0); }
            }
        }

        /// <summary>
        /// Attempts to load RenderDoc using system-default names and paths.
        /// </summary>
        /// <param name="renderDoc">If successful, this parameter contains a loaded <see cref="RenderDoc"/> instance.</param>
        /// <returns>Whether or not RenderDoc was successfully loaded.</returns>
        public static bool Load(out RenderDoc renderDoc) => Load(GetLibNames(), out renderDoc);

        /// Attempts to load RenderDoc from the given path.
        /// </summary>
        /// <param name="renderDocLibPath">The path to the RenderDoc shared library.</param>
        /// <param name="renderDoc">If successful, this parameter contains a loaded <see cref="RenderDoc"/> instance.</param>
        /// <returns>Whether or not RenderDoc was successfully loaded.</returns>
        public static bool Load(string renderDocLibPath, out RenderDoc renderDoc) => Load(new[] { renderDocLibPath }, out renderDoc);

        private static bool Load(string[] renderDocLibPaths, out RenderDoc renderDoc)
        {
            try
            {
                NativeLibrary lib = new NativeLibrary(renderDocLibPaths);
                renderDoc = new RenderDoc(lib);
                return true;
            }
            catch
            {
                renderDoc = null;
                return false;
            }
        }

        private static string[] GetLibNames()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                List<string> paths = new List<string>();
                string programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
                if (programFiles != null)
                {
                    string systemInstallPath = Path.Combine(programFiles, "RenderDoc", "renderdoc.dll");
                    if (File.Exists(systemInstallPath))
                    {
                        paths.Add(systemInstallPath);
                    }
                }
                paths.Add("renderdoc.dll");

                return paths.ToArray();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new[] { "librenderdoc.dylib" };
            }
            else
            {
                return new[] { "librenderdoc.so" };
            }
        }
    }
}
