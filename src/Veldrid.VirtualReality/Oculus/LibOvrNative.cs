using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeLibraryLoader;

namespace Veldrid.VirtualReality.Oculus
{
    internal static unsafe class LibOvrNative
    {
        private const string LibName32 = "LibOVRRT32_1.dll";
        private const string LibName64 = "LibOVRRT64_1.dll";

        private static readonly NativeLibrary s_libovrrt = LoadLibAndFunctions();

        internal static bool LibOvrLoadedSuccessfully() => s_libovrrt != null;

        private static NativeLibrary LoadLibAndFunctions()
        {
            string libName = Environment.Is64BitProcess ? LibName64 : LibName32;
            try
            {
                NativeLibrary lib = new NativeLibrary(libName);

                p_ovr_Initialize = lib.LoadFunction<ovr_Initialize_t>("ovr_Initialize");
                p_ovr_Shutdown = lib.LoadFunction<ovr_Shutdown_t>("ovr_Shutdown");
                p_ovr_GetLastErrorInfo = lib.LoadFunction<ovr_GetLastErrorInfo_t>("ovr_GetLastErrorInfo");
                p_ovr_Create = lib.LoadFunction<ovr_Create_t>("ovr_Create");
                p_ovr_Destroy = lib.LoadFunction<ovr_Destroy_t>("ovr_Destroy");
                p_ovr_GetHmdDesc = lib.LoadFunction<ovr_GetHmdDesc_t>("ovr_GetHmdDesc");
                p_ovr_GetTrackerCount = lib.LoadFunction<ovr_GetTrackerCount_t>("ovr_GetTrackerCount");
                p_ovr_GetFovTextureSize = lib.LoadFunction<ovr_GetFovTextureSize_t>("ovr_GetFovTextureSize");
                p_ovr_CreateTextureSwapChainDX = lib.LoadFunction<ovr_CreateTextureSwapChainDX_t>("ovr_CreateTextureSwapChainDX");
                p_ovr_GetTextureSwapChainLength = lib.LoadFunction<ovr_GetTextureSwapChainLength_t>("ovr_GetTextureSwapChainLength");
                p_ovr_GetTextureSwapChainBufferDX = lib.LoadFunction<ovr_GetTextureSwapChainBufferDX_t>("ovr_GetTextureSwapChainBufferDX");
                p_ovr_GetTextureSwapChainCurrentIndex = lib.LoadFunction<ovr_GetTextureSwapChainCurrentIndex_t>("ovr_GetTextureSwapChainCurrentIndex");
                p_ovr_DestroyTextureSwapChain = lib.LoadFunction<ovr_DestroyTextureSwapChain_t>("ovr_DestroyTextureSwapChain");
                p_ovr_CommitTextureSwapChain = lib.LoadFunction<ovr_CommitTextureSwapChain_t>("ovr_CommitTextureSwapChain");
                p_ovr_GetRenderDesc2 = lib.LoadFunction<ovr_GetRenderDesc2_t>("ovr_GetRenderDesc2");
                p_ovr_CalcEyePoses = lib.LoadFunction<ovr_CalcEyePoses_t>("ovr_CalcEyePoses");
                p_ovr_GetPredictedDisplayTime = lib.LoadFunction<ovr_GetPredictedDisplayTime_t>("ovr_GetPredictedDisplayTime");
                p_ovr_GetTrackingState = lib.LoadFunction<ovr_GetTrackingState_t>("ovr_GetTrackingState");
                p_ovr_CreateMirrorTextureWithOptionsDX = lib.LoadFunction<ovr_CreateMirrorTextureWithOptionsDX_t>("ovr_CreateMirrorTextureWithOptionsDX");
                p_ovr_SetTrackingOriginType = lib.LoadFunction<ovr_SetTrackingOriginType_t>("ovr_SetTrackingOriginType");
                p_ovr_GetSessionStatus = lib.LoadFunction<ovr_GetSessionStatus_t>("ovr_GetSessionStatus");
                p_ovr_RecenterTrackingOrigin = lib.LoadFunction<ovr_RecenterTrackingOrigin_t>("ovr_RecenterTrackingOrigin");
                p_ovrTimewarpProjectionDesc_FromProjection = lib.LoadFunction<ovrTimewarpProjectionDesc_FromProjection_t>("ovrTimewarpProjectionDesc_FromProjection");
                p_ovr_GetEyePoses = lib.LoadFunction<ovr_GetEyePoses_t>("ovr_GetEyePoses");
                p_ovr_SubmitFrame = lib.LoadFunction<ovr_SubmitFrame_t>("ovr_SubmitFrame");
                p_ovr_GetMirrorTextureBufferDX = lib.LoadFunction<ovr_GetMirrorTextureBufferDX_t>("ovr_GetMirrorTextureBufferDX");
                p_ovrMatrix4f_Projection = lib.LoadFunction<ovrMatrix4f_Projection_t>("ovrMatrix4f_Projection");
                p_ovr_GetTimeInSeconds = lib.LoadFunction<ovr_GetTimeInSeconds_t>("ovr_GetTimeInSeconds");
                p_ovr_CreateTextureSwapChainGL = lib.LoadFunction<ovr_CreateTextureSwapChainGL_t>("ovr_CreateTextureSwapChainGL");
                p_ovr_GetTextureSwapChainBufferGL = lib.LoadFunction<ovr_GetTextureSwapChainBufferGL_t>("ovr_GetTextureSwapChainBufferGL");
                p_ovr_CreateMirrorTextureWithOptionsGL = lib.LoadFunction<ovr_CreateMirrorTextureWithOptionsGL_t>("ovr_CreateMirrorTextureWithOptionsGL");
                p_ovr_GetMirrorTextureBufferGL = lib.LoadFunction<ovr_GetMirrorTextureBufferGL_t>("ovr_GetMirrorTextureBufferGL");
                p_ovr_GetInstanceExtensionsVk = lib.LoadFunction<ovr_GetInstanceExtensionsVk_t>("ovr_GetInstanceExtensionsVk");
                p_ovr_GetSessionPhysicalDeviceVk = lib.LoadFunction<ovr_GetSessionPhysicalDeviceVk_t>("ovr_GetSessionPhysicalDeviceVk");
                p_ovr_SetSynchonizationQueueVk = lib.LoadFunction<ovr_SetSynchonizationQueueVk_t>("ovr_SetSynchonizationQueueVk");
                p_ovr_CreateTextureSwapChainVk = lib.LoadFunction<ovr_CreateTextureSwapChainVk_t>("ovr_CreateTextureSwapChainVk");
                p_ovr_GetTextureSwapChainBufferVk = lib.LoadFunction<ovr_GetTextureSwapChainBufferVk_t>("ovr_GetTextureSwapChainBufferVk");
                p_ovr_CreateMirrorTextureWithOptionsVk = lib.LoadFunction<ovr_CreateMirrorTextureWithOptionsVk_t>("ovr_CreateMirrorTextureWithOptionsVk");
                p_ovr_GetMirrorTextureBufferVk = lib.LoadFunction<ovr_GetMirrorTextureBufferVk_t>("ovr_GetMirrorTextureBufferVk");
                p_ovr_GetDeviceExtensionsVk = lib.LoadFunction<ovr_GetDeviceExtensionsVk_t>("ovr_GetDeviceExtensionsVk");
                p_ovr_DestroyMirrorTexture = lib.LoadFunction<ovr_DestroyMirrorTexture_t>("ovr_DestroyMirrorTexture");

                return lib;
            }
            catch
            {
                return null;
            }
        }

        private delegate ovrResult ovr_Initialize_t(ovrInitParams* @params);
        private static ovr_Initialize_t p_ovr_Initialize;
        public static ovrResult ovr_Initialize(ovrInitParams* @params) => p_ovr_Initialize == null ? ovrResult.Error_LibLoad : p_ovr_Initialize(@params);

        private delegate void ovr_Shutdown_t();
        private static ovr_Shutdown_t p_ovr_Shutdown;
        public static void ovr_Shutdown() => p_ovr_Shutdown();

        private delegate void ovr_GetLastErrorInfo_t(out ovrErrorInfo errorInfo);
        private static ovr_GetLastErrorInfo_t p_ovr_GetLastErrorInfo;
        public static void ovr_GetLastErrorInfo(out ovrErrorInfo errorInfo) => p_ovr_GetLastErrorInfo(out errorInfo);

        private delegate ovrResult ovr_Create_t(ovrSession* pSession, ovrGraphicsLuid* pLuid);
        private static ovr_Create_t p_ovr_Create;
        public static ovrResult ovr_Create(ovrSession* pSession, ovrGraphicsLuid* pLuid) => p_ovr_Create(pSession, pLuid);

        private delegate void ovr_Destroy_t(ovrSession session);
        private static ovr_Destroy_t p_ovr_Destroy;
        public static void ovr_Destroy(ovrSession session) => p_ovr_Destroy(session);

        private delegate ovrHmdDesc ovr_GetHmdDesc_t(ovrSession session);
        private static ovr_GetHmdDesc_t p_ovr_GetHmdDesc;
        public static ovrHmdDesc ovr_GetHmdDesc(ovrSession session) => p_ovr_GetHmdDesc(session);

        private delegate uint ovr_GetTrackerCount_t(ovrSession session);
        private static ovr_GetTrackerCount_t p_ovr_GetTrackerCount;
        public static uint ovr_GetTrackerCount(ovrSession session) => p_ovr_GetTrackerCount(session);

        private delegate ovrSizei ovr_GetFovTextureSize_t(ovrSession session, ovrEyeType eye, ovrFovPort fov, float pixelsPerDisplayPixel);
        private static ovr_GetFovTextureSize_t p_ovr_GetFovTextureSize;
        public static ovrSizei ovr_GetFovTextureSize(ovrSession session, ovrEyeType eye, ovrFovPort fov, float pixelsPerDisplayPixel)
            => p_ovr_GetFovTextureSize(session, eye, fov, pixelsPerDisplayPixel);

        private delegate ovrResult ovr_CreateTextureSwapChainDX_t(
            ovrSession session,
            IntPtr d3dPtr,
            ovrTextureSwapChainDesc* desc,
            ovrTextureSwapChain* outTextureSet);
        private static ovr_CreateTextureSwapChainDX_t p_ovr_CreateTextureSwapChainDX;
        public static ovrResult ovr_CreateTextureSwapChainDX(
            ovrSession session,
            IntPtr d3dPtr,
            ovrTextureSwapChainDesc* desc,
            ovrTextureSwapChain* outTextureSet) => p_ovr_CreateTextureSwapChainDX(session, d3dPtr, desc, outTextureSet);

        private delegate ovrResult ovr_GetTextureSwapChainLength_t(ovrSession session, ovrTextureSwapChain chain, int* length);
        private static ovr_GetTextureSwapChainLength_t p_ovr_GetTextureSwapChainLength;
        public static ovrResult ovr_GetTextureSwapChainLength(ovrSession session, ovrTextureSwapChain chain, int* length)
            => p_ovr_GetTextureSwapChainLength(session, chain, length);

        private delegate ovrResult ovr_GetTextureSwapChainBufferDX_t(
            ovrSession session,
            ovrTextureSwapChain chain,
            int index,
            Guid iid,
            IntPtr* ppObject);
        private static ovr_GetTextureSwapChainBufferDX_t p_ovr_GetTextureSwapChainBufferDX;
        public static ovrResult ovr_GetTextureSwapChainBufferDX(
            ovrSession session,
            ovrTextureSwapChain chain,
            int index,
            Guid iid,
            IntPtr* ppObject) => p_ovr_GetTextureSwapChainBufferDX(session, chain, index, iid, ppObject);

        private delegate ovrResult ovr_GetTextureSwapChainCurrentIndex_t(ovrSession session, ovrTextureSwapChain chain, int* currentIndex);
        private static ovr_GetTextureSwapChainCurrentIndex_t p_ovr_GetTextureSwapChainCurrentIndex;
        public static ovrResult ovr_GetTextureSwapChainCurrentIndex(ovrSession session, ovrTextureSwapChain chain, int* currentIndex)
            => p_ovr_GetTextureSwapChainCurrentIndex(session, chain, currentIndex);

        private delegate void ovr_DestroyTextureSwapChain_t(ovrSession session, ovrTextureSwapChain chain);
        private static ovr_DestroyTextureSwapChain_t p_ovr_DestroyTextureSwapChain;
        public static void ovr_DestroyTextureSwapChain(ovrSession session, ovrTextureSwapChain chain)
            => p_ovr_DestroyTextureSwapChain(session, chain);

        private delegate ovrResult ovr_CommitTextureSwapChain_t(ovrSession session, ovrTextureSwapChain chain);
        private static ovr_CommitTextureSwapChain_t p_ovr_CommitTextureSwapChain;
        public static ovrResult ovr_CommitTextureSwapChain(ovrSession session, ovrTextureSwapChain chain) => p_ovr_CommitTextureSwapChain(session, chain);

        private delegate ovrEyeRenderDesc ovr_GetRenderDesc2_t(ovrSession session, ovrEyeType eyeType, ovrFovPort fov);
        private static ovr_GetRenderDesc2_t p_ovr_GetRenderDesc2;
        public static ovrEyeRenderDesc ovr_GetRenderDesc2(ovrSession session, ovrEyeType eyeType, ovrFovPort fov)
            => p_ovr_GetRenderDesc2(session, eyeType, fov);

        private delegate void ovr_CalcEyePoses_t(ovrPosef headPose, Vector3* hmdToEyeOffset, ovrPosef* outEyePoses);
        private static ovr_CalcEyePoses_t p_ovr_CalcEyePoses;
        public static void ovr_CalcEyePoses(ovrPosef headPose, Vector3* hmdToEyeOffset, ovrPosef* outEyePoses)
            => p_ovr_CalcEyePoses(headPose, hmdToEyeOffset, outEyePoses);

        private delegate double ovr_GetPredictedDisplayTime_t(ovrSession session, long frameIndex);
        private static ovr_GetPredictedDisplayTime_t p_ovr_GetPredictedDisplayTime;
        public static double ovr_GetPredictedDisplayTime(ovrSession session, long frameIndex)
            => p_ovr_GetPredictedDisplayTime(session, frameIndex);

        private delegate ovrTrackingState ovr_GetTrackingState_t(ovrSession session, double absTime, ovrBool latencyMarker);
        private static ovr_GetTrackingState_t p_ovr_GetTrackingState;
        public static ovrTrackingState ovr_GetTrackingState(ovrSession session, double absTime, ovrBool latencyMarker)
            => p_ovr_GetTrackingState(session, absTime, latencyMarker);

        private delegate ovrResult ovr_CreateMirrorTextureWithOptionsDX_t(
            ovrSession session,
            IntPtr d3dPtr,
            ovrMirrorTextureDesc* desc,
            ovrMirrorTexture* outMirrorTexture);
        private static ovr_CreateMirrorTextureWithOptionsDX_t p_ovr_CreateMirrorTextureWithOptionsDX;
        public static ovrResult ovr_CreateMirrorTextureWithOptionsDX(
            ovrSession session,
            IntPtr d3dPtr,
            ovrMirrorTextureDesc* desc,
            ovrMirrorTexture* outMirrorTexture) => p_ovr_CreateMirrorTextureWithOptionsDX(session, d3dPtr, desc, outMirrorTexture);

        private delegate ovrResult ovr_SetTrackingOriginType_t(ovrSession session, ovrTrackingOrigin origin);
        private static ovr_SetTrackingOriginType_t p_ovr_SetTrackingOriginType;
        public static ovrResult ovr_SetTrackingOriginType(ovrSession session, ovrTrackingOrigin origin)
            => p_ovr_SetTrackingOriginType(session, origin);

        private delegate ovrResult ovr_GetSessionStatus_t(ovrSession session, ovrSessionStatus* sessionStatus);
        private static ovr_GetSessionStatus_t p_ovr_GetSessionStatus;
        public static ovrResult ovr_GetSessionStatus(ovrSession session, ovrSessionStatus* sessionStatus)
            => p_ovr_GetSessionStatus(session, sessionStatus);

        private delegate ovrResult ovr_RecenterTrackingOrigin_t(ovrSession session);
        private static ovr_RecenterTrackingOrigin_t p_ovr_RecenterTrackingOrigin;
        public static ovrResult ovr_RecenterTrackingOrigin(ovrSession session)
            => p_ovr_RecenterTrackingOrigin(session);

        private delegate ovrTimewarpProjectionDesc ovrTimewarpProjectionDesc_FromProjection_t(Matrix4x4 Projection, ovrProjectionModifier projectionModFlags);
        private static ovrTimewarpProjectionDesc_FromProjection_t p_ovrTimewarpProjectionDesc_FromProjection;
        public static ovrTimewarpProjectionDesc ovrTimewarpProjectionDesc_FromProjection(Matrix4x4 Projection, ovrProjectionModifier projectionModFlags)
            => p_ovrTimewarpProjectionDesc_FromProjection(Projection, projectionModFlags);

        private delegate void ovr_GetEyePoses_t(
            ovrSession session,
            long frameIndex,
            ovrBool latencyMarker,
            EyePair_Vector3* hmdToEyeOffset,
            out EyePair_ovrPosef outEyePoses,
            double* outSensorSampleTime);
        private static ovr_GetEyePoses_t p_ovr_GetEyePoses;
        public static void ovr_GetEyePoses(
            ovrSession session,
            long frameIndex,
            ovrBool latencyMarker,
            EyePair_Vector3* hmdToEyeOffset,
            out EyePair_ovrPosef outEyePoses,
            double* outSensorSampleTime) => p_ovr_GetEyePoses(session, frameIndex, latencyMarker, hmdToEyeOffset, out outEyePoses, outSensorSampleTime);

        private delegate ovrResult ovr_SubmitFrame_t(
            ovrSession session,
            long frameIndex,
            void* viewScaleDesc,
            ovrLayerHeader** layerPtrList,
            uint layerCount);
        private static ovr_SubmitFrame_t p_ovr_SubmitFrame;
        public static ovrResult ovr_SubmitFrame(
            ovrSession session,
            long frameIndex,
            void* viewScaleDesc,
            ovrLayerHeader** layerPtrList,
            uint layerCount) => p_ovr_SubmitFrame(session, frameIndex, viewScaleDesc, layerPtrList, layerCount);

        private delegate ovrResult ovr_GetMirrorTextureBufferDX_t(ovrSession session, ovrMirrorTexture mirror, Guid iid, IntPtr* ppObject);
        private static ovr_GetMirrorTextureBufferDX_t p_ovr_GetMirrorTextureBufferDX;
        public static ovrResult ovr_GetMirrorTextureBufferDX(ovrSession session, ovrMirrorTexture mirror, Guid iid, IntPtr* ppObject)
            => p_ovr_GetMirrorTextureBufferDX(session, mirror, iid, ppObject);

        private delegate Matrix4x4 ovrMatrix4f_Projection_t(ovrFovPort fov, float znear, float zfar, ovrProjectionModifier projectionModFlags);
        private static ovrMatrix4f_Projection_t p_ovrMatrix4f_Projection;
        public static Matrix4x4 ovrMatrix4f_Projection(ovrFovPort fov, float znear, float zfar, ovrProjectionModifier projectionModFlags)
            => p_ovrMatrix4f_Projection(fov, znear, zfar, projectionModFlags);

        private delegate double ovr_GetTimeInSeconds_t();
        private static ovr_GetTimeInSeconds_t p_ovr_GetTimeInSeconds;
        public static double ovr_GetTimeInSeconds() => p_ovr_GetTimeInSeconds();

        private delegate ovrResult ovr_CreateTextureSwapChainGL_t(
            ovrSession session,
            ovrTextureSwapChainDesc* desc,
            ovrTextureSwapChain* out_TextureSwapChain);
        private static ovr_CreateTextureSwapChainGL_t p_ovr_CreateTextureSwapChainGL;
        public static ovrResult ovr_CreateTextureSwapChainGL(
            ovrSession session,
            ovrTextureSwapChainDesc* desc,
            ovrTextureSwapChain* out_TextureSwapChain) => p_ovr_CreateTextureSwapChainGL(session, desc, out_TextureSwapChain);

        private delegate ovrResult ovr_GetTextureSwapChainBufferGL_t(
            ovrSession session,
            ovrTextureSwapChain chain,
            int index, uint* out_TexId);
        private static ovr_GetTextureSwapChainBufferGL_t p_ovr_GetTextureSwapChainBufferGL;
        public static ovrResult ovr_GetTextureSwapChainBufferGL(
            ovrSession session,
            ovrTextureSwapChain chain,
            int index,
            uint* out_TexId) => p_ovr_GetTextureSwapChainBufferGL(session, chain, index, out_TexId);

        private delegate ovrResult ovr_CreateMirrorTextureWithOptionsGL_t(
            ovrSession session,
            ovrMirrorTextureDesc* desc,
            ovrMirrorTexture* out_MirrorTexture);
        private static ovr_CreateMirrorTextureWithOptionsGL_t p_ovr_CreateMirrorTextureWithOptionsGL;
        public static ovrResult ovr_CreateMirrorTextureWithOptionsGL(
            ovrSession session,
            ovrMirrorTextureDesc* desc,
            ovrMirrorTexture* out_MirrorTexture) => p_ovr_CreateMirrorTextureWithOptionsGL(session, desc, out_MirrorTexture);

        private delegate ovrResult ovr_GetMirrorTextureBufferGL_t(
            ovrSession session,
            ovrMirrorTexture mirrorTexture,
            uint* out_TexId);
        private static ovr_GetMirrorTextureBufferGL_t p_ovr_GetMirrorTextureBufferGL;
        public static ovrResult ovr_GetMirrorTextureBufferGL(
            ovrSession session,
            ovrMirrorTexture mirrorTexture,
            uint* out_TexId) => p_ovr_GetMirrorTextureBufferGL(session, mirrorTexture, out_TexId);

        private delegate ovrResult ovr_GetInstanceExtensionsVk_t(ovrGraphicsLuid luid, byte* extensionNames, uint* inoutExtensionNamesSize);
        private static ovr_GetInstanceExtensionsVk_t p_ovr_GetInstanceExtensionsVk;
        public static ovrResult ovr_GetInstanceExtensionsVk(ovrGraphicsLuid luid, byte* extensionNames, uint* inoutExtensionNamesSize)
            => p_ovr_GetInstanceExtensionsVk(luid, extensionNames, inoutExtensionNamesSize);

        private delegate ovrResult ovr_GetDeviceExtensionsVk_t(ovrGraphicsLuid luid, byte* extensionNames, uint* inoutExtensionNamesSize);
        private static ovr_GetDeviceExtensionsVk_t p_ovr_GetDeviceExtensionsVk;
        public static ovrResult ovr_GetDeviceExtensionsVk(ovrGraphicsLuid luid, byte* extensionNames, uint* inoutExtensionNamesSize)
            => p_ovr_GetDeviceExtensionsVk(luid, extensionNames, inoutExtensionNamesSize);

        private delegate ovrResult ovr_GetSessionPhysicalDeviceVk_t(
            ovrSession session,
            ovrGraphicsLuid luid,
            IntPtr instance,
            IntPtr* out_physicalDevice);
        private static ovr_GetSessionPhysicalDeviceVk_t p_ovr_GetSessionPhysicalDeviceVk;
        public static ovrResult ovr_GetSessionPhysicalDeviceVk(
            ovrSession session,
            ovrGraphicsLuid luid,
            IntPtr instance,
            IntPtr* out_physicalDevice) => p_ovr_GetSessionPhysicalDeviceVk(session, luid, instance, out_physicalDevice);

        private delegate ovrResult ovr_SetSynchonizationQueueVk_t(ovrSession session, IntPtr queue);
        private static ovr_SetSynchonizationQueueVk_t p_ovr_SetSynchonizationQueueVk;
        public static ovrResult ovr_SetSynchonizationQueueVk(ovrSession session, IntPtr queue)
            => p_ovr_SetSynchonizationQueueVk(session, queue);

        private delegate ovrResult ovr_CreateTextureSwapChainVk_t(
            ovrSession session,
            IntPtr device,
            ovrTextureSwapChainDesc* desc,
            ovrTextureSwapChain* out_TextureSwapChain);
        private static ovr_CreateTextureSwapChainVk_t p_ovr_CreateTextureSwapChainVk;
        public static ovrResult ovr_CreateTextureSwapChainVk(
            ovrSession session,
            IntPtr device,
            ovrTextureSwapChainDesc* desc,
            ovrTextureSwapChain* out_TextureSwapChain)
            => p_ovr_CreateTextureSwapChainVk(session, device, desc, out_TextureSwapChain);

        private delegate ovrResult ovr_GetTextureSwapChainBufferVk_t(
            ovrSession session,
            ovrTextureSwapChain chain,
            int index,
            ulong* out_Image);
        private static ovr_GetTextureSwapChainBufferVk_t p_ovr_GetTextureSwapChainBufferVk;
        public static ovrResult ovr_GetTextureSwapChainBufferVk(
            ovrSession session,
            ovrTextureSwapChain chain,
            int index,
            ulong* out_Image) => p_ovr_GetTextureSwapChainBufferVk(session, chain, index, out_Image);

        private delegate ovrResult ovr_CreateMirrorTextureWithOptionsVk_t(
            ovrSession session,
            IntPtr device,
            ovrMirrorTextureDesc* desc,
            ovrMirrorTexture* out_MirrorTexture);
        private static ovr_CreateMirrorTextureWithOptionsVk_t p_ovr_CreateMirrorTextureWithOptionsVk;
        public static ovrResult ovr_CreateMirrorTextureWithOptionsVk(
            ovrSession session,
            IntPtr device,
            ovrMirrorTextureDesc* desc,
            ovrMirrorTexture* out_MirrorTexture) => p_ovr_CreateMirrorTextureWithOptionsVk(session, device, desc, out_MirrorTexture);

        private delegate ovrResult ovr_GetMirrorTextureBufferVk_t(
            ovrSession session,
            ovrMirrorTexture mirrorTexture,
            ulong* out_Image);
        private static ovr_GetMirrorTextureBufferVk_t p_ovr_GetMirrorTextureBufferVk;
        public static ovrResult ovr_GetMirrorTextureBufferVk(
            ovrSession session,
            ovrMirrorTexture mirrorTexture,
            ulong* out_Image) => p_ovr_GetMirrorTextureBufferVk(session, mirrorTexture, out_Image);

        private delegate void ovr_DestroyMirrorTexture_t(ovrSession session, ovrMirrorTexture mirrorTexture);
        private static ovr_DestroyMirrorTexture_t p_ovr_DestroyMirrorTexture;
        public static void ovr_DestroyMirrorTexture(ovrSession session, ovrMirrorTexture mirrorTexture)
            => p_ovr_DestroyMirrorTexture(session, mirrorTexture);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrInitParams
    {
        public ovrInitFlags Flags;
        public uint RequestedMinorVersion;
        public IntPtr LogCallback;
        public UIntPtr UserData;
        public uint ConnectionTimeoutMS;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrErrorInfo
    {
        public ovrResult Result;
        public FixedString512 ErrorString;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct FixedString24
    {
        public fixed byte Data[24];

        public override string ToString()
        {
            fixed (byte* ptr = Data)
            {
                return Util.GetUtf8String(ptr);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct FixedString64
    {
        public fixed byte Data[64];

        public override string ToString()
        {
            fixed (byte* ptr = Data)
            {
                return Util.GetUtf8String(ptr);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct FixedString128
    {
        public fixed byte Data[128];

        public override string ToString()
        {
            fixed (byte* ptr = Data)
            {
                return Util.GetUtf8String(ptr);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct FixedString512
    {
        public fixed byte Data[512];

        public override string ToString()
        {
            fixed (byte* ptr = Data)
            {
                return Util.GetUtf8String(ptr);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ovrGraphicsLuid
    {
        public fixed byte Reserved[8];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrSession
    {
        public readonly IntPtr NativePtr;
        public bool IsNull => NativePtr == IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ovrHmdDesc
    {
        private IntPtr _padHack0;
        /// <summary>
        /// The type of HMD.
        /// </summary>
        public ovrHmdType Type
        {
            get
            {
                fixed (IntPtr* padHackPtr = &_padHack0)
                {
                    return Unsafe.Read<ovrHmdType>(padHackPtr);
                }
            }
        }
        /// <summary>
        /// UTF8-encoded product identification string (e.g. "Oculus Rift DK1").
        /// </summary>
        public FixedString64 ProductName;
        /// <summary>
        /// UTF8-encoded HMD manufacturer identification string.
        /// </summary>
        public FixedString64 Manufacturer;
        /// <summary>
        /// HID (USB) vendor identifier of the device.
        /// </summary>
        public short VendorId;
        /// <summary>
        /// HID (USB) product identifier of the device.
        /// </summary>
        public short ProductId;
        /// <summary>
        /// HMD serial number.
        /// </summary>
        public FixedString24 SerialNumber;
        /// <summary>
        /// HMD firmware major version.
        /// </summary>
        public short FirmwareMajor;
        /// <summary>
        /// HMD firmware minor version.
        /// </summary>
        public short FirmwareMinor;
        /// <summary>
        /// Available ovrHmdCaps bits.
        /// </summary>
        public uint AvailableHmdCaps;
        /// <summary>
        /// Default ovrHmdCaps bits.
        /// </summary>
        public uint DefaultHmdCaps;
        /// <summary>
        /// Available ovrTrackingCaps bits.
        /// </summary>
        public uint AvailableTrackingCaps;
        /// <summary>
        /// Default ovrTrackingCaps bits.
        /// </summary>
        public uint DefaultTrackingCaps;
        /// <summary>
        /// Defines the recommended FOVs for the HMD.
        /// </summary>
        public EyePair_ovrFovPort DefaultEyeFov;
        /// <summary>
        /// Defines the maximum FOVs for the HMD.
        /// </summary>
        public ovrFovPort MaxEyeFov_0;
        /// <summary>
        /// Defines the maximum FOVs for the HMD.
        /// </summary>
        public ovrFovPort MaxEyeFov_1;
        /// <summary>
        /// Resolution of the full HMD screen (both eyes) in pixels.
        /// </summary>
        public ovrSizei Resolution;
        private IntPtr _padHack1;
        /// <summary>
        /// Refresh rate of the display in cycles per second.
        /// </summary>
        public float DisplayRefreshRate
        {
            get
            {
                fixed (IntPtr* padHackPtr = &_padHack1)
                {
                    return Unsafe.Read<float>(padHackPtr);
                }
            }
        }
    }

    internal enum ovrHmdType
    {
        None = 0,
        DK1 = 3,
        DKHD = 4,
        DK2 = 6,
        CB = 8,
        Other = 9,
        E3_2015 = 10,
        ES06 = 11,
        ES09 = 12,
        ES11 = 13,
        CV1 = 14,
    }

    /// <summary>
    /// Describes the up, down, left, and right angles of the field of view.
    /// Field Of View (FOV) tangent of the angle units.
    /// NOTE: For a standard 90 degree vertical FOV, we would
    /// have: { UpTan = tan(90 degrees / 2), DownTan = tan(90 degrees / 2) }.
    /// </summary>

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrFovPort
    {
        /// <summary>
        /// Tangent of the angle between the viewing vector and top edge of the FOV.
        /// </summary>
        public float UpTan;
        /// <summary>
        /// Tangent of the angle between the viewing vector and bottom edge of the FOV.
        /// </summary>
        public float DownTan;
        /// <summary>
        /// Tangent of the angle between the viewing vector and left edge of the FOV.
        /// </summary>
        public float LeftTan;
        /// <summary>
        /// Tangent of the angle between the viewing vector and right edge of the FOV.
        /// </summary>
        public float RightTan;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrSizei
    {
        public int w;
        public int h;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrTextureSwapChainDesc
    {
        /// <summary>
        /// Must be 2D
        /// </summary>
        public ovrTextureType Type;
        public ovrTextureFormat Format;
        /// <summary>
        /// Must be 6 for Cube, 1 for other types.
        /// </summary>
        public int ArraySize;
        public int Width;
        public int Height;
        public int MipLevels;
        public int SampleCount;
        /// <summary>
        /// Not buffered in a chain. For images that don't change
        /// </summary>
        public ovrBool StaticImage;
        public ovrTextureMiscFlags MiscFlags;
        /// <summary>
        /// Not used for GL.
        /// </summary>
        public ovrTextureBindFlags BindFlags;
    }

    [Flags]
    internal enum ovrTextureMiscFlags
    {
        None,

        /// <summary>
        /// Vulkan and DX only: The underlying texture is created with a TYPELESS equivalent
        /// of the format specified in the texture desc. The SDK will still access the
        /// texture using the format specified in the texture desc, but the app can
        /// create views with different formats if this is specified.
        /// </summary>
        DX_Typeless = 0x0001,

        /// <summary>
        /// DX only: Allow generation of the mip chain on the GPU via the GenerateMips
        /// call. This flag requires that RenderTarget binding also be specified.
        /// </summary>
        AllowGenerateMips = 0x0002,

        /// <summary>
        /// Texture swap chain contains protected content, and requires
        /// HDCP connection in order to display to HMD. Also prevents
        /// mirroring or other redirection of any frame containing this contents
        /// </summary>
        ProtectedContent = 0x0004,

        /// <summary>
        /// Automatically generate and use the mip chain in composition on each submission.
        /// Mips are regenerated from highest quality level, ignoring other pre-existing mip levels.
        /// Not supported for depth or compressed (BC) formats.
        /// </summary>
        AutoGenerateMips = 0x0008,
    }

    internal enum ovrTextureBindFlags
    {
        None,

        /// <summary>
        /// The application can write into the chain with pixel shader.
        /// </summary>
        DX_RenderTarget = 0x0001,

        /// <summary>
        /// The application can write to the chain with compute shader.
        /// </summary>
        DX_UnorderedAccess = 0x0002,

        /// <summary>
        /// The chain buffers can be bound as depth and/or stencil buffers.
        /// This flag cannot be combined with DX_RenderTarget.
        /// </summary>
        DX_DepthStencil = 0x0004,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrBool
    {
        public readonly byte Value;
        public static implicit operator bool(ovrBool b8) => b8.Value != 0;
        public static implicit operator ovrBool(bool b) => new ovrBool(b);

        public ovrBool(bool value)
        {
            Value = value ? (byte)1 : (byte)0;
        }

        public ovrBool(byte value)
        {
            Value = value;
        }

        public override string ToString() => string.Format("{0} [{1}]", (bool)this, Value);
    }

    internal enum ovrTextureType
    {
        /// <summary>
        /// 2D textures.
        /// </summary>
        Texture2D,
        /// <summary>
        /// Application-provided 2D texture. Not supported on PC.
        /// </summary>
        Texture2D_External,
        /// <summary>
        /// Cube maps. ovrTextureSwapChainDesc::ArraySize must be 6 for this type.
        /// </summary>
        TextureCube,
    }

    internal enum ovrTextureFormat
    {
        UNKNOWN = 0,
        /// <summary>
        /// Not currently supported on PC. Requires a DirectX 11.1 device.
        /// </summary>
        B5G6R5_UNORM = 1,
        /// <summary>
        /// Not currently supported on PC. Requires a DirectX 11.1 device.
        /// </summary>
        B5G5R5A1_UNORM = 2,
        /// <summary>
        /// Not currently supported on PC. Requires a DirectX 11.1 device.
        /// </summary>
        B4G4R4A4_UNORM = 3,
        R8G8B8A8_UNORM = 4,
        R8G8B8A8_UNORM_SRGB = 5,
        B8G8R8A8_UNORM = 6,
        B8G8R8_UNORM = 27,
        /// <summary>
        /// Not supported for OpenGL applications
        /// </summary>
        B8G8R8A8_UNORM_SRGB = 7,
        /// <summary>
        /// Not supported for OpenGL applications
        /// </summary>
        B8G8R8X8_UNORM = 8,
        /// <summary>
        /// Not supported for OpenGL applications
        /// </summary>
        B8G8R8X8_UNORM_SRGB = 9,
        R16G16B16A16_FLOAT = 10,
        /// <summary>
        /// Introduced in v1.10
        /// </summary>
        R11G11B10_FLOAT = 25,

        // Depth formats
        D16_UNORM = 11,
        D24_UNORM_S8_UINT = 12,
        D32_FLOAT = 13,
        D32_FLOAT_S8X24_UINT = 14,

        // Added in 1.5 compressed formats can be used for static layers
        BC1_UNORM = 15,
        BC1_UNORM_SRGB = 16,
        BC2_UNORM = 17,
        BC2_UNORM_SRGB = 18,
        BC3_UNORM = 19,
        BC3_UNORM_SRGB = 20,
        BC6H_UF16 = 21,
        BC6H_SF16 = 22,
        BC7_UNORM = 23,
        BC7_UNORM_SRGB = 24,
    }

    internal enum ovrEyeType
    {
        /// <summary>
        /// left eye, from the viewer's perspective.
        /// </summary>
        Left = 0,
        /// <summary>
        /// The right eye, from the viewer's perspective.
        /// </summary>
        Right = 1,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrTextureSwapChain
    {
        public readonly IntPtr NativePtr;
        public bool IsNull => NativePtr == IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrMirrorTexture
    {
        public readonly IntPtr NativePtr;
        public bool IsNull => NativePtr == IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrEyeRenderDesc
    {
        /// <summary>
        /// The eye index to which this instance corresponds.
        /// </summary>
        public ovrEyeType Eye;
        /// <summary>
        /// The field of view.
        /// </summary>
        public ovrFovPort Fov;
        /// <summary>
        /// Distortion viewport.
        /// </summary>
        public ovrRecti DistortedViewport;
        /// <summary>
        /// How many display pixels will fit in tan(angle) = 1.
        /// </summary>
        public Vector2 PixelsPerTanAngleAtCenter;
        /// <summary>
        /// Transform of eye from the HMD center, in meters.
        /// </summary>
        public ovrPosef HmdToEyePose;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrRecti
    {
        public ovrVector2i Pos;
        public ovrSizei Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrVector2i
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrPosef
    {
        public Quaternion Orientation;
        public Vector3 Position;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ovrTrackingState
    {
        /// <summary>
        /// Predicted head pose (and derivatives) at the requested absolute time.
        /// </summary>
        public ovrPoseStatef HeadPose;

        /// <summary>
        /// HeadPose tracking status described by ovrStatusBits.
        /// </summary>
        public ovrStatusBits StatusFlags;

        /// <summary>
        /// The most recent calculated pose for each hand when hand controller tracking is present.
        /// HandPoses[ovrHand_Left] refers to the left hand and HandPoses[ovrHand_Right] to the right.
        /// These values can be combined with ovrInputState for complete hand controller information.
        /// </summary>
        public ovrPoseStatef HandPoses_Left;

        /// <summary>
        /// The most recent calculated pose for each hand when hand controller tracking is present.
        /// HandPoses[ovrHand_Left] refers to the left hand and HandPoses[ovrHand_Right] to the right.
        /// These values can be combined with ovrInputState for complete hand controller information.
        /// </summary>
        public ovrPoseStatef HandPoses_Right;

        /// <summary>
        /// HandPoses status flags described by ovrStatusBits.
        /// Only ovrStatus_OrientationTracked and ovrStatus_PositionTracked are reported.
        /// </summary>
        public ovrStatusBits HandStatusFlags_Left;

        /// <summary>
        /// HandPoses status flags described by ovrStatusBits.
        /// Only ovrStatus_OrientationTracked and ovrStatus_PositionTracked are reported.
        /// </summary>
        public ovrStatusBits HandStatusFlags_Right;

        /// The pose of the origin captured during calibration.
        /// Like all other poses here, this is expressed in the space set by ovr_RecenterTrackingOrigin,
        /// or ovr_SpecifyTrackingOrigin and so will change every time either of those functions are
        /// called. This pose can be used to calculate where the calibrated origin lands in the new
        /// recentered space. If an application never calls ovr_RecenterTrackingOrigin or
        /// ovr_SpecifyTrackingOrigin, expect this value to be the identity pose and as such will point
        /// respective origin based on ovrTrackingOrigin requested when calling ovr_GetTrackingState.
        public ovrPosef CalibratedOrigin;
    }

    [Flags]
    internal enum ovrStatusBits
    {
        /// <summary>
        /// Orientation is currently tracked (connected & in use).
        /// </summary>
        OrientationTracked = 0x0001,
        /// <summary>
        /// Position is currently tracked (false if out of range).
        /// </summary>
        PositionTracked = 0x0002,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrPoseStatef
    {
        /// <summary>
        /// Position and orientation.
        /// </summary>
        public ovrPosef ThePose;
        /// <summary>
        /// Angular velocity in radians per second.
        /// </summary>
        public Vector3 AngularVelocity;
        /// <summary>
        /// Velocity in meters per second.
        /// </summary>
        public Vector3 LinearVelocity;
        /// <summary>
        /// Angular acceleration in radians per second per second.
        /// </summary>
        public Vector3 AngularAcceleration;
        /// <summary>
        /// Acceleration in meters per second per second.
        /// </summary>
        public Vector3 LinearAcceleration;
        /// <summary>
        /// Absolute time that this pose refers to. See ovr_GetTimeInSeconds
        /// </summary>
        public double TimeInSeconds;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrMirrorTextureDesc
    {
        public ovrTextureFormat Format;
        public int Width;
        public int Height;
        public ovrTextureMiscFlags MiscFlags;
        public ovrMirrorOptions MirrorOptions;
    }

    [Flags]
    internal enum ovrMirrorOptions
    {
        /// <summary>
        /// By default the mirror texture will be:
        /// * Pre-distortion (i.e. rectilinear)
        /// * Contain both eye textures
        /// * Exclude Guardian, Notifications, System Menu GUI
        /// </summary>
        Default = 0x0000,

        /// <summary>
        /// Retrieves the barrel distorted texture contents instead of the rectilinear one
        /// This is only recommended for debugging purposes, and not for final desktop presentation
        /// </summary>
        PostDistortion = 0x0001,


        /// <summary>
        /// Since Default renders both eyes into the mirror texture,
        /// these two flags are exclusive (i.e. cannot use them simultaneously)
        /// </summary>
        LeftEyeOnly = 0x0002,
        RightEyeOnly = 0x0004,

        /// <summary>
        /// Shows the boundary system aka Guardian on the mirror texture
        /// </summary>
        IncludeGuardian = 0x0008,

        /// <summary>
        /// Shows system notifications the user receives on the mirror texture
        /// </summary>
        IncludeNotifications = 0x0010,

        /// <summary>
        /// Shows the system menu (triggered by hitting the Home button) on the mirror texture
        /// </summary>
        IncludeSystemGui = 0x0020,

        /// <summary>
        /// Forces mirror output to use max symmetric FOV instead of asymmetric full FOV used by HMD.
        /// Only valid for rectilinear mirrors i.e. using PostDistortion with
        /// ForceSymmetricFov will result in ovrError_InvalidParameter error.
        /// </summary>
        ForceSymmetricFov = 0x0040,
    }

    internal enum ovrTrackingOrigin
    {
        /// <summary>
        /// Tracking system origin reported at eye (HMD) height
        /// Prefer using this origin when your application requires
        /// matching user's current physical head pose to a virtual head pose
        /// without any regards to a the height of the floor. Cockpit-based,
        /// or 3rd-person experiences are ideal candidates.
        /// When used, all poses in ovrTrackingState are reported as an offset
        /// transform from the profile calibrated or recentered HMD pose.
        /// It is recommended that apps using this origin type call ovr_RecenterTrackingOrigin
        /// prior to starting the VR experience, but notify the user before doing so
        /// to make sure the user is in a comfortable pose, facing a comfortable
        /// direction.
        /// </summary>
        EyeLevel = 0,

        /// <summary>
        /// Tracking system origin reported at floor height
        /// Prefer using this origin when your application requires the
        /// physical floor height to match the virtual floor height, such as
        /// standing experiences.
        /// When used, all poses in ovrTrackingState are reported as an offset
        /// transform from the profile calibrated floor pose. Calling ovr_RecenterTrackingOrigin
        /// will recenter the X & Z axes as well as yaw, but the Y-axis (i.e. height) will continue
        /// to be reported using the floor height as the origin for all poses.
        /// </summary>
        FloorLevel = 1,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrSessionStatus
    {
        /// <summary>
        /// True if the process has VR focus and thus is visible in the HMD.
        /// </summary>
        public ovrBool IsVisible;

        /// <summary>
        /// True if an HMD is present.
        /// </summary>
        public ovrBool HmdPresent;

        /// <summary>
        /// True if the HMD is on the user's head.
        /// </summary>
        public ovrBool HmdMounted;

        /// <summary>
        /// True if the session is in a display-lost state. See ovr_SubmitFrame.
        /// </summary>
        public ovrBool DisplayLost;

        /// <summary>
        /// True if the application should initiate shutdown.
        /// </summary>
        public ovrBool ShouldQuit;

        /// <summary>
        /// True if UX has requested re-centering. Must call ovr_ClearShouldRecenterFlag,
        /// ovr_RecenterTrackingOrigin or ovr_SpecifyTrackingOrigin.
        /// </summary>
        public ovrBool ShouldRecenter;

        /// <summary>
        /// True if the application is the foreground application and receives input (e.g. Touch
        /// controller state). If this is false then the application is in the background (but possibly
        /// still visible) should hide any input representations such as hands.
        /// </summary>
        public ovrBool HasInputFocus;

        /// <summary>
        /// True if a system overlay is present, such as a dashboard. In this case the application
        /// (if visible) should pause while still drawing, avoid drawing near-field graphics so they
        /// don't visually fight with the system overlay, and consume fewer CPU and GPU resources.
        /// Deprecated. Do not use.
        /// </summary>
        [Obsolete]
        public ovrBool OverlayPresent;

        /// <summary>
        /// True if runtime is requesting that the application provide depth buffers with projection
        /// layers.
        /// </summary>
        public ovrBool DepthRequested;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrLayerEyeFovDepth
    {
        /// <summary>
        /// Header.Type must be EyeFovDepth.
        /// </summary>
        public ovrLayerHeader Header;

        /// <summary>
        /// ovrTextureSwapChains for the left and right eye respectively.
        /// The second one of which can be NULL for cases described above.
        /// </summary>
        public EyePair_ovrTextureSwapChain ColorTexture;

        /// <summary>
        /// Specifies the ColorTexture sub-rect UV coordinates.
        /// Both Viewport[0] and Viewport[1] must be valid.
        /// </summary>
        public EyePair_ovrRecti Viewport;

        /// <summary>
        /// The viewport field of view.
        /// </summary>
        public EyePair_ovrFovPort Fov;

        /// <summary>
        /// Specifies the position and orientation of each eye view, with position specified in meters.
        /// RenderPose will typically be the value returned from ovr_CalcEyePoses,
        /// but can be different in special cases if a different head pose is used for rendering.
        /// </summary>
        public EyePair_ovrPosef RenderPose;

        /// <summary>
        /// Specifies the timestamp when the source ovrPosef (used in calculating RenderPose)
        /// was sampled from the SDK. Typically retrieved by calling ovr_GetTimeInSeconds
        /// around the instant the application calls ovr_GetTrackingState
        /// The main purpose for this is to accurately track app tracking latency.
        /// </summary>
        public double SensorSampleTime;

        /// <summary>
        /// Depth texture for depth composition with overlays
        /// Must map 1:1 to the ColorTexture.
        /// </summary>
        public EyePair_ovrTextureSwapChain DepthTexture;

        /// <summary>
        /// Specifies how to convert DepthTexture information into meters.
        /// See ovrTimewarpProjectionDesc_FromProjection
        /// </summary>
        public ovrTimewarpProjectionDesc ProjectionDesc;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrLayerHeader
    {
        public ovrLayerType Type;
        public ovrLayerFlags Flags;
        public FixedString128 Reserved;
    }

    [Flags]
    internal enum ovrLayerFlags
    {
        None = 0,

        /// <summary>
        /// HighQuality enables 4x anisotropic sampling during the composition of the layer.
        /// The benefits are mostly visible at the periphery for high-frequency & high-contrast visuals.
        /// For best results consider combining this flag with an ovrTextureSwapChain that has mipmaps and
        /// instead of using arbitrary sized textures, prefer texture sizes that are powers-of-two.
        /// Actual rendered viewport and doesn't necessarily have to fill the whole texture.
        /// </summary>
        HighQuality = 0x01,

        /// <summary>
        /// TextureOriginAtBottomLeft: the opposite is TopLeft.
        /// Generally this is false for D3D, true for OpenGL.
        /// </summary>
        TextureOriginAtBottomLeft = 0x02,

        /// <summary>
        /// Mark this surface as "headlocked", which means it is specified
        /// relative to the HMD and moves with it, rather than being specified
        /// relative to sensor/torso space and remaining still while the head moves.
        /// What used to be QuadHeadLocked is now Quad plus this flag.
        /// However the flag can be applied to any layer type to achieve a similar effect.
        /// </summary>
        HeadLocked = 0x04,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct EyePair_Vector3
    {
        public Vector3 Left;
        public Vector3 Right;

        public EyePair_Vector3(Vector3 left, Vector3 right)
        {
            Left = left;
            Right = right;
        }

        public Vector3 this[int index]
        {
            get
            {
                Debug.Assert(index == 0 || index == 1);
                if (index == 0) { return Left; }
                else { return Right; }
            }
            set
            {
                Debug.Assert(index == 0 || index == 1);
                if (index == 0) { Left = value; }
                else { Right = value; }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct EyePair_ovrPosef
    {
        public ovrPosef Left;
        public ovrPosef Right;

        public EyePair_ovrPosef(ovrPosef left, ovrPosef right)
        {
            Left = left;
            Right = right;
        }

        public ovrPosef this[int index]
        {
            get
            {
                Debug.Assert(index == 0 || index == 1);
                if (index == 0) { return Left; }
                else { return Right; }
            }
            set
            {
                Debug.Assert(index == 0 || index == 1);
                if (index == 0) { Left = value; }
                else { Right = value; }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct EyePair_ovrFovPort
    {
        public ovrFovPort Left;
        public ovrFovPort Right;

        public ovrFovPort this[int index]
        {
            get
            {
                Debug.Assert(index == 0 || index == 1);
                if (index == 0) { return Left; }
                else { return Right; }
            }
            set
            {
                Debug.Assert(index == 0 || index == 1);
                if (index == 0) { Left = value; }
                else { Right = value; }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct EyePair_ovrTextureSwapChain
    {
        public ovrTextureSwapChain Left;
        public ovrTextureSwapChain Right;

        public ovrTextureSwapChain this[int index]
        {
            get
            {
                Debug.Assert(index == 0 || index == 1);
                if (index == 0) { return Left; }
                else { return Right; }
            }
            set
            {
                Debug.Assert(index == 0 || index == 1);
                if (index == 0) { Left = value; }
                else { Right = value; }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct EyePair_ovrRecti
    {
        public ovrRecti Left;
        public ovrRecti Right;

        public ovrRecti this[int index]
        {
            get
            {
                Debug.Assert(index == 0 || index == 1);
                if (index == 0) { return Left; }
                else { return Right; }
            }
            set
            {
                Debug.Assert(index == 0 || index == 1);
                if (index == 0) { Left = value; }
                else { Right = value; }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrTimewarpProjectionDesc
    {
        /// <summary>
        /// Projection matrix element [2][2].
        /// </summary>
        public float Projection22;
        /// <summary>
        /// Projection matrix element [2][3].
        /// </summary>
        public float Projection23;
        /// <summary>
        /// Projection matrix element [3][2].
        /// </summary>
        public float Projection32;
    }

    internal enum ovrLayerType
    {
        /// <summary>
        /// Layer is disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Described by ovrLayerEyeFov.
        /// </summary>
        EyeFov = 1,

        /// <summary>
        /// Described by ovrLayerEyeFovDepth.
        /// </summary>
        EyeFovDepth = 2,

        /// <summary>
        /// Described by ovrLayerQuad. Previously called QuadInWorld.
        /// </summary>
        Quad = 3,

        /// <summary>
        /// Described by ovrLayerEyeMatrix.
        /// </summary>
        EyeMatrix = 5,

        /// <summary>
        /// Described by ovrLayerEyeFovMultires.
        /// </summary>
        EyeFovMultires = 7,

        /// <summary>
        /// Described by ovrLayerCylinder.
        /// </summary>
        Cylinder = 8,

        /// <summary>
        /// Described by ovrLayerCube
        /// </summary>
        Cube = 10,
    }

    [Flags]
    internal enum ovrProjectionModifier
    {
        /// <summary>
        /// Use for generating a default projection matrix that is:
        /// * Right-handed.
        /// * Near depth values stored in the depth buffer are smaller than far depth values.
        /// * Both near and far are explicitly defined.
        /// * With a clipping range that is (0 to w).
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Enable if using left-handed transformations in your application.
        /// </summary>
        LeftHanded = 0x01,

        /// <summary>
        /// After the projection transform is applied, far values stored in the depth buffer will be less
        /// than closer depth values.
        /// NOTE: Enable only if the application is using a floating-point depth buffer for proper
        /// precision.
        /// </summary>
        FarLessThanNear = 0x02,

        /// <summary>
        /// When this flag is used, the zfar value pushed into ovrMatrix4f_Projection() will be ignored
        /// NOTE: Enable only if FarLessThanNear is also enabled where the far clipping
        /// plane will be pushed to infinity.
        /// </summary>
        FarClipAtInfinity = 0x04,

        /// <summary>
        /// Enable if the application is rendering with OpenGL and expects a projection matrix with a
        /// clipping range of (-w to w).
        /// Ignore this flag if your application already handles the conversion from D3D range (0 to w) to
        /// OpenGL.
        /// </summary>
        ClipRangeOpenGL = 0x08,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ovrViewScaleDesc
    {
        /// <summary>
        /// Transform of each eye from the HMD center, in meters.
        /// </summary>
        public EyePair_ovrPosef HmdToEyePose;
        /// <summary>
        /// Ratio of viewer units to meter units.
        /// </summary>
        public float HmdSpaceToWorldScaleInMeters;
    }
}
