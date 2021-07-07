using System;
using System.Runtime.InteropServices;

namespace Veldrid.OpenGLBinding
{
    // uint = uint
    // GLuint = uint
    // GLuint64 = uint64
    // GLenum = uint
    // Glclampf = 32-bit float, [0, 1]
    public static unsafe class OpenGLNative
    {
        private static Func<string, IntPtr> s_getProcAddress;

        private const CallingConvention CallConv = CallingConvention.Winapi;

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGenVertexArrays_t(uint n, out uint arrays);
        private static glGenVertexArrays_t p_glGenVertexArrays;
        public static void glGenVertexArrays(uint n, out uint arrays) => p_glGenVertexArrays(n, out arrays);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate uint glGetError_t();
        private static glGetError_t p_glGetError;
        public static uint glGetError() => p_glGetError();

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBindVertexArray_t(uint array);
        private static glBindVertexArray_t p_glBindVertexArray;
        public static void glBindVertexArray(uint array) => p_glBindVertexArray(array);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glClearColor_t(float red, float green, float blue, float alpha);
        private static glClearColor_t p_glClearColor;
        public static void glClearColor(float red, float green, float blue, float alpha)
            => p_glClearColor(red, green, blue, alpha);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDrawBuffer_t(DrawBufferMode mode);
        private static glDrawBuffer_t p_glDrawBuffer;
        public static void glDrawBuffer(DrawBufferMode mode) => p_glDrawBuffer(mode);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDrawBuffers_t(uint n, DrawBuffersEnum* bufs);
        private static glDrawBuffers_t p_glDrawBuffers;
        public static void glDrawBuffers(uint n, DrawBuffersEnum* bufs) => p_glDrawBuffers(n, bufs);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glClear_t(ClearBufferMask mask);
        private static glClear_t p_glClear;
        public static void glClear(ClearBufferMask mask) => p_glClear(mask);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glClearDepth_t(double depth);
        private static glClearDepth_t p_glClearDepth;
        public static void glClearDepth(double depth) => p_glClearDepth(depth);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glClearDepthf_t(float depth);
        private static glClearDepthf_t p_glClearDepthf;
        public static void glClearDepthf(float depth) => p_glClearDepthf(depth);

        private static glClearDepthf_t p_glClearDepthf_Compat;
        public static void glClearDepth_Compat(float depth) => p_glClearDepthf_Compat(depth);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDrawElements_t(PrimitiveType mode, uint count, DrawElementsType type, void* indices);
        private static glDrawElements_t p_glDrawElements;
        public static void glDrawElements(PrimitiveType mode, uint count, DrawElementsType type, void* indices)
            => p_glDrawElements(mode, count, type, indices);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDrawElementsBaseVertex_t(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            int basevertex);
        private static glDrawElementsBaseVertex_t p_glDrawElementsBaseVertex;
        public static void glDrawElementsBaseVertex(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            int basevertex) => p_glDrawElementsBaseVertex(mode, count, type, indices, basevertex);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDrawElementsInstanced_t(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            uint primcount);
        private static glDrawElementsInstanced_t p_glDrawElementsInstanced;
        public static void glDrawElementsInstanced(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            uint primcount) => p_glDrawElementsInstanced(mode, count, type, indices, primcount);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDrawElementsInstancedBaseVertex_t(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            uint primcount,
            int basevertex);
        private static glDrawElementsInstancedBaseVertex_t p_glDrawElementsInstancedBaseVertex;
        public static void glDrawElementsInstancedBaseVertex(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            uint primcount,
            int basevertex) => p_glDrawElementsInstancedBaseVertex(mode, count, type, indices, primcount, basevertex);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDrawElementsInstancedBaseVertexBaseInstance_t(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            uint primcount,
            int basevertex,
            uint baseinstance);
        private static glDrawElementsInstancedBaseVertexBaseInstance_t p_glDrawElementsInstancedBaseVertexBaseInstance;
        public static void glDrawElementsInstancedBaseVertexBaseInstance(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            uint primcount,
            int basevertex,
            uint baseinstance)
            => p_glDrawElementsInstancedBaseVertexBaseInstance(
                mode, count, type, indices, primcount, basevertex, baseinstance);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDrawArrays_t(PrimitiveType mode, int first, uint count);
        private static glDrawArrays_t p_glDrawArrays;
        public static void glDrawArrays(PrimitiveType mode, int first, uint count) => p_glDrawArrays(mode, first, count);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDrawArraysInstanced_t(PrimitiveType mode, int first, uint count, uint primcount);
        private static glDrawArraysInstanced_t p_glDrawArraysInstanced;
        public static void glDrawArraysInstanced(PrimitiveType mode, int first, uint count, uint primcount)
            => p_glDrawArraysInstanced(mode, first, count, primcount);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDrawArraysInstancedBaseInstance_t(
            PrimitiveType mode,
            int first,
            uint count,
            uint primcount,
            uint baseinstance);
        private static glDrawArraysInstancedBaseInstance_t p_glDrawArraysInstancedBaseInstance;
        public static void glDrawArraysInstancedBaseInstance(
            PrimitiveType mode,
            int first,
            uint count,
            uint primcount,
            uint baseinstance) => p_glDrawArraysInstancedBaseInstance(mode, first, count, primcount, baseinstance);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGenBuffers_t(uint n, out uint buffers);
        private static glGenBuffers_t p_glGenBuffers;
        public static void glGenBuffers(uint n, out uint buffers) => p_glGenBuffers(n, out buffers);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDeleteBuffers_t(uint n, ref uint buffers);
        private static glDeleteBuffers_t p_glDeleteBuffers;
        public static void glDeleteBuffers(uint n, ref uint buffers) => p_glDeleteBuffers(n, ref buffers);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGenFramebuffers_t(uint n, out uint ids);
        private static glGenFramebuffers_t p_glGenFramebuffers;
        public static void glGenFramebuffers(uint n, out uint ids) => p_glGenFramebuffers(n, out ids);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glActiveTexture_t(TextureUnit texture);
        private static glActiveTexture_t p_glActiveTexture;
        public static void glActiveTexture(TextureUnit texture) => p_glActiveTexture(texture);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glFramebufferTexture1D_t(
            FramebufferTarget target,
            GLFramebufferAttachment attachment,
            TextureTarget textarget,
            uint texture,
            int level);
        private static glFramebufferTexture1D_t p_glFramebufferTexture1D;
        public static void glFramebufferTexture1D(
            FramebufferTarget target,
            GLFramebufferAttachment attachment,
            TextureTarget textarget,
            uint texture,
            int level) => p_glFramebufferTexture1D(target, attachment, textarget, texture, level);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glFramebufferTexture2D_t(
            FramebufferTarget target,
            GLFramebufferAttachment attachment,
            TextureTarget textarget,
            uint texture,
            int level);
        private static glFramebufferTexture2D_t p_glFramebufferTexture2D;
        public static void glFramebufferTexture2D(
            FramebufferTarget target,
            GLFramebufferAttachment attachment,
            TextureTarget textarget,
            uint texture,
            int level) => p_glFramebufferTexture2D(target, attachment, textarget, texture, level);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBindTexture_t(TextureTarget target, uint texture);
        private static glBindTexture_t p_glBindTexture;
        public static void glBindTexture(TextureTarget target, uint texture) => p_glBindTexture(target, texture);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBindFramebuffer_t(FramebufferTarget target, uint framebuffer);
        private static glBindFramebuffer_t p_glBindFramebuffer;
        public static void glBindFramebuffer(FramebufferTarget target, uint framebuffer)
            => p_glBindFramebuffer(target, framebuffer);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDeleteFramebuffers_t(uint n, ref uint framebuffers);
        private static glDeleteFramebuffers_t p_glDeleteFramebuffers;
        public static void glDeleteFramebuffers(uint n, ref uint framebuffers) => p_glDeleteFramebuffers(n, ref framebuffers);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGenTextures_t(uint n, out uint textures);
        private static glGenTextures_t p_glGenTextures;
        public static void glGenTextures(uint n, out uint textures) => p_glGenTextures(n, out textures);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDeleteTextures_t(uint n, ref uint textures);
        private static glDeleteTextures_t p_glDeleteTextures;
        public static void glDeleteTextures(uint n, ref uint textures) => p_glDeleteTextures(n, ref textures);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate FramebufferErrorCode glCheckFramebufferStatus_t(FramebufferTarget target);
        private static glCheckFramebufferStatus_t p_glCheckFramebufferStatus;
        public static FramebufferErrorCode glCheckFramebufferStatus(FramebufferTarget target)
            => p_glCheckFramebufferStatus(target);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBindBuffer_t(BufferTarget target, uint buffer);
        private static glBindBuffer_t p_glBindBuffer;
        public static void glBindBuffer(BufferTarget target, uint buffer) => p_glBindBuffer(target, buffer);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glViewportIndexedf_t(uint index, float x, float y, float w, float h);
        private static glViewportIndexedf_t p_glViewportIndexedf;
        public static void glViewportIndexed(uint index, float x, float y, float w, float h)
            => p_glViewportIndexedf(index, x, y, w, h);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glViewport_t(int x, int y, uint width, uint height);
        private static glViewport_t p_glViewport;
        public static void glViewport(int x, int y, uint width, uint height) => p_glViewport(x, y, width, height);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDepthRangeIndexed_t(uint index, double nearVal, double farVal);
        private static glDepthRangeIndexed_t p_glDepthRangeIndexed;
        public static void glDepthRangeIndexed(uint index, double nearVal, double farVal)
            => p_glDepthRangeIndexed(index, nearVal, farVal);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDepthRangef_t(float n, float f);
        private static glDepthRangef_t p_glDepthRangef;
        public static void glDepthRangef(float n, float f) => p_glDepthRangef(n, f);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBufferSubData_t(BufferTarget target, IntPtr offset, UIntPtr size, void* data);
        private static glBufferSubData_t p_glBufferSubData;
        public static void glBufferSubData(BufferTarget target, IntPtr offset, UIntPtr size, void* data)
            => p_glBufferSubData(target, offset, size, data);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glNamedBufferSubData_t(uint buffer, IntPtr offset, uint size, void* data);
        private static glNamedBufferSubData_t p_glNamedBufferSubData;
        public static void glNamedBufferSubData(uint buffer, IntPtr offset, uint size, void* data)
            => p_glNamedBufferSubData(buffer, offset, size, data);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glScissorIndexed_t(uint index, int left, int bottom, uint width, uint height);
        private static glScissorIndexed_t p_glScissorIndexed;
        public static void glScissorIndexed(uint index, int left, int bottom, uint width, uint height)
            => p_glScissorIndexed(index, left, bottom, width, height);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glScissor_t(int x, int y, uint width, uint height);
        private static glScissor_t p_glScissor;
        public static void glScissor(int x, int y, uint width, uint height) => p_glScissor(x, y, width, height);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glPixelStorei_t(PixelStoreParameter pname, int param);
        private static glPixelStorei_t p_glPixelStorei;
        public static void glPixelStorei(PixelStoreParameter pname, int param) => p_glPixelStorei(pname, param);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexSubImage1D_t(
            TextureTarget target,
            int level,
            int xoffset,
            uint width,
            GLPixelFormat format,
            GLPixelType type,
            void* pixels);
        private static glTexSubImage1D_t p_glTexSubImage1D;
        public static void glTexSubImage1D(
            TextureTarget target,
            int level,
            int xoffset,
            uint width,
            GLPixelFormat format,
            GLPixelType type,
            void* pixels) => p_glTexSubImage1D(target, level, xoffset, width, format, type, pixels);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexSubImage2D_t(
            TextureTarget target,
            int level,
            int xoffset,
            int yoffset,
            uint width,
            uint height,
            GLPixelFormat format,
            GLPixelType type,
            void* pixels);
        private static glTexSubImage2D_t p_glTexSubImage2D;
        public static void glTexSubImage2D(
            TextureTarget target,
            int level,
            int xoffset,
            int yoffset,
            uint width,
            uint height,
            GLPixelFormat format,
            GLPixelType type,
            void* pixels) => p_glTexSubImage2D(target, level, xoffset, yoffset, width, height, format, type, pixels);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexSubImage3D_t(
            TextureTarget target,
            int level,
            int xoffset,
            int yoffset,
            int zoffset,
            uint width,
            uint height,
            uint depth,
            GLPixelFormat format,
            GLPixelType type,
            void* pixels);
        private static glTexSubImage3D_t p_glTexSubImage3D;
        public static void glTexSubImage3D(
            TextureTarget target,
            int level,
            int xoffset,
            int yoffset,
            int zoffset,
            uint width,
            uint height,
            uint depth,
            GLPixelFormat format,
            GLPixelType type,
            void* pixels)
            => p_glTexSubImage3D(target, level, xoffset, yoffset, zoffset, width, height, depth, format, type, pixels);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glShaderSource_t(uint shader, uint count, byte** @string, int* length);
        private static glShaderSource_t p_glShaderSource;
        public static void glShaderSource(uint shader, uint count, byte** @string, int* length)
            => p_glShaderSource(shader, count, @string, length);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate uint glCreateShader_t(ShaderType shaderType);
        private static glCreateShader_t p_glCreateShader;
        public static uint glCreateShader(ShaderType shaderType) => p_glCreateShader(shaderType);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glCompileShader_t(uint shader);
        private static glCompileShader_t p_glCompileShader;
        public static void glCompileShader(uint shader) => p_glCompileShader(shader);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetShaderiv_t(uint shader, ShaderParameter pname, int* @params);
        private static glGetShaderiv_t p_glGetShaderiv;
        public static void glGetShaderiv(uint shader, ShaderParameter pname, int* @params)
            => p_glGetShaderiv(shader, pname, @params);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetShaderInfoLog_t(uint shader, uint maxLength, uint* length, byte* infoLog);
        private static glGetShaderInfoLog_t p_glGetShaderInfoLog;
        public static void glGetShaderInfoLog(uint shader, uint maxLength, uint* length, byte* infoLog)
            => p_glGetShaderInfoLog(shader, maxLength, length, infoLog);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDeleteShader_t(uint shader);
        private static glDeleteShader_t p_glDeleteShader;
        public static void glDeleteShader(uint shader) => p_glDeleteShader(shader);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGenSamplers_t(uint n, out uint samplers);
        private static glGenSamplers_t p_glGenSamplers;
        public static void glGenSamplers(uint n, out uint samplers) => p_glGenSamplers(n, out samplers);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glSamplerParameterf_t(uint sampler, SamplerParameterName pname, float param);
        private static glSamplerParameterf_t p_glSamplerParameterf;
        public static void glSamplerParameterf(uint sampler, SamplerParameterName pname, float param)
            => p_glSamplerParameterf(sampler, pname, param);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glSamplerParameteri_t(uint sampler, SamplerParameterName pname, int param);
        private static glSamplerParameteri_t p_glSamplerParameteri;
        public static void glSamplerParameteri(uint sampler, SamplerParameterName pname, int param)
            => p_glSamplerParameteri(sampler, pname, param);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glSamplerParameterfv_t(uint sampler, SamplerParameterName pname, float* @params);
        private static glSamplerParameterfv_t p_glSamplerParameterfv;
        public static void glSamplerParameterfv(uint sampler, SamplerParameterName pname, float* @params)
            => p_glSamplerParameterfv(sampler, pname, @params);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBindSampler_t(uint unit, uint sampler);
        private static glBindSampler_t p_glBindSampler;
        public static void glBindSampler(uint unit, uint sampler) => p_glBindSampler(unit, sampler);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDeleteSamplers_t(uint n, ref uint samplers);
        private static glDeleteSamplers_t p_glDeleteSamplers;
        public static void glDeleteSamplers(uint n, ref uint samplers) => p_glDeleteSamplers(n, ref samplers);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBlendFuncSeparatei_t(
            uint buf,
            BlendingFactorSrc srcRGB,
            BlendingFactorDest dstRGB,
            BlendingFactorSrc srcAlpha,
            BlendingFactorDest dstAlpha);
        private static glBlendFuncSeparatei_t p_glBlendFuncSeparatei;
        public static void glBlendFuncSeparatei(
            uint buf,
            BlendingFactorSrc srcRGB,
            BlendingFactorDest dstRGB,
            BlendingFactorSrc srcAlpha,
            BlendingFactorDest dstAlpha) => p_glBlendFuncSeparatei(buf, srcRGB, dstRGB, srcAlpha, dstAlpha);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBlendFuncSeparate_t(
            BlendingFactorSrc srcRGB,
            BlendingFactorDest dstRGB,
            BlendingFactorSrc srcAlpha,
            BlendingFactorDest dstAlpha);
        private static glBlendFuncSeparate_t p_glBlendFuncSeparate;
        public static void glBlendFuncSeparate(
            BlendingFactorSrc srcRGB,
            BlendingFactorDest dstRGB,
            BlendingFactorSrc srcAlpha,
            BlendingFactorDest dstAlpha) => p_glBlendFuncSeparate(srcRGB, dstRGB, srcAlpha, dstAlpha);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glEnable_t(EnableCap cap);
        private static glEnable_t p_glEnable;
        public static void glEnable(EnableCap cap) => p_glEnable(cap);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glEnablei_t(EnableCap cap, uint index);
        private static glEnablei_t p_glEnablei;
        public static void glEnablei(EnableCap cap, uint index) => p_glEnablei(cap, index);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDisable_t(EnableCap cap);
        private static glDisable_t p_glDisable;
        public static void glDisable(EnableCap cap) => p_glDisable(cap);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDisablei_t(EnableCap cap, uint index);
        private static glDisablei_t p_glDisablei;
        public static void glDisablei(EnableCap cap, uint index) => p_glDisablei(cap, index);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBlendEquationSeparatei_t(uint buf, BlendEquationMode modeRGB, BlendEquationMode modeAlpha);
        private static glBlendEquationSeparatei_t p_glBlendEquationSeparatei;
        public static void glBlendEquationSeparatei(uint buf, BlendEquationMode modeRGB, BlendEquationMode modeAlpha)
            => p_glBlendEquationSeparatei(buf, modeRGB, modeAlpha);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBlendEquationSeparate_t(BlendEquationMode modeRGB, BlendEquationMode modeAlpha);
        private static glBlendEquationSeparate_t p_glBlendEquationSeparate;
        public static void glBlendEquationSeparate(BlendEquationMode modeRGB, BlendEquationMode modeAlpha)
            => p_glBlendEquationSeparate(modeRGB, modeAlpha);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBlendColor_t(float red, float green, float blue, float alpha);
        private static glBlendColor_t p_glBlendColor;
        public static void glBlendColor(float red, float green, float blue, float alpha)
            => p_glBlendColor(red, green, blue, alpha);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDepthFunc_t(DepthFunction func);
        private static glDepthFunc_t p_glDepthFunc;
        public static void glDepthFunc(DepthFunction func) => p_glDepthFunc(func);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDepthMask_t(GLboolean flag);
        private static glDepthMask_t p_glDepthMask;
        public static void glDepthMask(GLboolean flag) => p_glDepthMask(flag);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glCullFace_t(CullFaceMode mode);
        private static glCullFace_t p_glCullFace;
        public static void glCullFace(CullFaceMode mode) => p_glCullFace(mode);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glPolygonMode_t(MaterialFace face, PolygonMode mode);
        private static glPolygonMode_t p_glPolygonMode;
        public static void glPolygonMode(MaterialFace face, PolygonMode mode) => p_glPolygonMode(face, mode);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glPolygonOffset_t(float factor, float units);
        private static glPolygonOffset_t p_glPolygonOffset;
        public static void glPolygonOffset(float factor, float units) => p_glPolygonOffset(factor, units);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate uint glCreateProgram_t();
        private static glCreateProgram_t p_glCreateProgram;
        public static uint glCreateProgram() => p_glCreateProgram();

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glAttachShader_t(uint program, uint shader);
        private static glAttachShader_t p_glAttachShader;
        public static void glAttachShader(uint program, uint shader) => p_glAttachShader(program, shader);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBindAttribLocation_t(uint program, uint index, byte* name);
        private static glBindAttribLocation_t p_glBindAttribLocation;
        public static void glBindAttribLocation(uint program, uint index, byte* name)
            => p_glBindAttribLocation(program, index, name);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glLinkProgram_t(uint program);
        private static glLinkProgram_t p_glLinkProgram;
        public static void glLinkProgram(uint program) => p_glLinkProgram(program);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetProgramiv_t(uint program, GetProgramParameterName pname, int* @params);
        private static glGetProgramiv_t p_glGetProgramiv;
        public static void glGetProgramiv(uint program, GetProgramParameterName pname, int* @params)
            => p_glGetProgramiv(program, pname, @params);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetProgramInfoLog_t(uint program, uint maxLength, uint* length, byte* infoLog);
        private static glGetProgramInfoLog_t p_glGetProgramInfoLog;
        public static void glGetProgramInfoLog(uint program, uint maxLength, uint* length, byte* infoLog)
            => p_glGetProgramInfoLog(program, maxLength, length, infoLog);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glUniformBlockBinding_t(uint program, uint uniformBlockIndex, uint uniformBlockBinding);
        private static glUniformBlockBinding_t p_glUniformBlockBinding;
        public static void glUniformBlockBinding(uint program, uint uniformBlockIndex, uint uniformBlockBinding)
            => p_glUniformBlockBinding(program, uniformBlockIndex, uniformBlockBinding);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDeleteProgram_t(uint program);
        private static glDeleteProgram_t p_glDeleteProgram;
        public static void glDeleteProgram(uint program) => p_glDeleteProgram(program);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glUniform1i_t(int location, int v0);
        private static glUniform1i_t p_glUniform1i;
        public static void glUniform1i(int location, int v0) => p_glUniform1i(location, v0);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate uint glGetUniformBlockIndex_t(uint program, byte* uniformBlockName);
        private static glGetUniformBlockIndex_t p_glGetUniformBlockIndex;
        public static uint glGetUniformBlockIndex(uint program, byte* uniformBlockName)
            => p_glGetUniformBlockIndex(program, uniformBlockName);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate int glGetUniformLocation_t(uint program, byte* name);
        private static glGetUniformLocation_t p_glGetUniformLocation;
        public static int glGetUniformLocation(uint program, byte* name) => p_glGetUniformLocation(program, name);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate int glGetAttribLocation_t(uint program, byte* name);
        private static glGetAttribLocation_t p_glGetAttribLocation;
        public static int glGetAttribLocation(uint program, byte* name) => p_glGetAttribLocation(program, name);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glUseProgram_t(uint program);
        private static glUseProgram_t p_glUseProgram;
        public static void glUseProgram(uint program) => p_glUseProgram(program);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBindBufferRange_t(
            BufferRangeTarget target,
            uint index,
            uint buffer,
            IntPtr offset,
            UIntPtr size);
        private static glBindBufferRange_t p_glBindBufferRange;
        public static void glBindBufferRange(
            BufferRangeTarget target,
            uint index,
            uint buffer,
            IntPtr offset,
            UIntPtr size) => p_glBindBufferRange(target, index, buffer, offset, size);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void DebugProc(
            DebugSource source,
            DebugType type,
            uint id,
            DebugSeverity severity,
            uint length,
            byte* message,
            void* userParam);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDebugMessageCallback_t(DebugProc callback, void* userParam);
        private static glDebugMessageCallback_t p_glDebugMessageCallback;
        public static void glDebugMessageCallback(DebugProc callback, void* userParam)
            => p_glDebugMessageCallback(callback, userParam);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBufferData_t(BufferTarget target, UIntPtr size, void* data, BufferUsageHint usage);
        private static glBufferData_t p_glBufferData;
        public static void glBufferData(BufferTarget target, UIntPtr size, void* data, BufferUsageHint usage)
            => p_glBufferData(target, size, data, usage);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glNamedBufferData_t(uint buffer, uint size, void* data, BufferUsageHint usage);
        private static glNamedBufferData_t p_glNamedBufferData;
        public static void glNamedBufferData(uint buffer, uint size, void* data, BufferUsageHint usage)
            => p_glNamedBufferData(buffer, size, data, usage);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexImage1D_t(
            TextureTarget target,
            int level,
            PixelInternalFormat internalFormat,
            uint width,
            int border,
            GLPixelFormat format,
            GLPixelType type,
            void* data);
        private static glTexImage1D_t p_glTexImage1D;
        public static void glTexImage1D(
            TextureTarget target,
            int level,
            PixelInternalFormat internalFormat,
            uint width,
            int border,
            GLPixelFormat format,
            GLPixelType type,
            void* data) => p_glTexImage1D(target, level, internalFormat, width, border, format, type, data);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexImage2D_t(
            TextureTarget target,
            int level,
            PixelInternalFormat internalFormat,
            uint width,
            uint height,
            int border,
            GLPixelFormat format,
            GLPixelType type,
            void* data);
        private static glTexImage2D_t p_glTexImage2D;
        public static void glTexImage2D(
            TextureTarget target,
            int level,
            PixelInternalFormat internalFormat,
            uint width,
            uint height,
            int border,
            GLPixelFormat format,
            GLPixelType type,
            void* data) => p_glTexImage2D(target, level, internalFormat, width, height, border, format, type, data);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexImage3D_t(
            TextureTarget target,
            int level,
            PixelInternalFormat internalFormat,
            uint width,
            uint height,
            uint depth,
            int border,
            GLPixelFormat format,
            GLPixelType type,
            void* data);
        private static glTexImage3D_t p_glTexImage3D;
        public static void glTexImage3D(
            TextureTarget target,
            int level,
            PixelInternalFormat internalFormat,
            uint width,
            uint height,
            uint depth,
            int border,
            GLPixelFormat format,
            GLPixelType type,
            void* data) => p_glTexImage3D(target, level, internalFormat, width, height, depth, border, format, type, data);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glEnableVertexAttribArray_t(uint index);
        private static glEnableVertexAttribArray_t p_glEnableVertexAttribArray;
        public static void glEnableVertexAttribArray(uint index) => p_glEnableVertexAttribArray(index);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDisableVertexAttribArray_t(uint index);
        private static glDisableVertexAttribArray_t p_glDisableVertexAttribArray;
        public static void glDisableVertexAttribArray(uint index) => p_glDisableVertexAttribArray(index);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glVertexAttribPointer_t(
            uint index,
            int size,
            VertexAttribPointerType type,
            GLboolean normalized,
            uint stride,
            void* pointer);
        private static glVertexAttribPointer_t p_glVertexAttribPointer;
        public static void glVertexAttribPointer(
            uint index,
            int size,
            VertexAttribPointerType type,
            GLboolean normalized,
            uint stride,
            void* pointer) => p_glVertexAttribPointer(index, size, type, normalized, stride, pointer);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glVertexAttribIPointer_t(
            uint index,
            int size,
            VertexAttribPointerType type,
            uint stride,
            void* pointer);
        private static glVertexAttribIPointer_t p_glVertexAttribIPointer;
        public static void glVertexAttribIPointer(
            uint index,
            int size,
            VertexAttribPointerType type,
            uint stride,
            void* pointer) => p_glVertexAttribIPointer(index, size, type, stride, pointer);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glVertexAttribDivisor_t(uint index, uint divisor);
        private static glVertexAttribDivisor_t p_glVertexAttribDivisor;
        public static void glVertexAttribDivisor(uint index, uint divisor) => p_glVertexAttribDivisor(index, divisor);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glFrontFace_t(FrontFaceDirection mode);
        private static glFrontFace_t p_glFrontFace;
        public static void glFrontFace(FrontFaceDirection mode) => p_glFrontFace(mode);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetIntegerv_t(GetPName pname, int* data);
        private static glGetIntegerv_t p_glGetIntegerv;
        public static void glGetIntegerv(GetPName pname, int* data) => p_glGetIntegerv(pname, data);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBindTextureUnit_t(uint unit, uint texture);
        private static glBindTextureUnit_t p_glBindTextureUnit;
        public static void glBindTextureUnit(uint unit, uint texture) => p_glBindTextureUnit(unit, texture);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexParameteri_t(TextureTarget target, TextureParameterName pname, int param);
        private static glTexParameteri_t p_glTexParameteri;
        public static void glTexParameteri(TextureTarget target, TextureParameterName pname, int param)
            => p_glTexParameteri(target, pname, param);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate byte* glGetString_t(StringName name);
        private static glGetString_t p_glGetString;
        public static byte* glGetString(StringName name) => p_glGetString(name);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate byte* glGetStringi_t(StringNameIndexed name, uint index);
        private static glGetStringi_t p_glGetStringi;
        public static byte* glGetStringi(StringNameIndexed name, uint index) => p_glGetStringi(name, index);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glObjectLabel_t(ObjectLabelIdentifier identifier, uint name, uint length, byte* label);
        private static glObjectLabel_t p_glObjectLabel;
        public static void glObjectLabel(ObjectLabelIdentifier identifier, uint name, uint length, byte* label)
            => p_glObjectLabel(identifier, name, length, label);

        /// <summary>
        /// Indicates whether the glObjectLabel function was successfully loaded.
        /// Some drivers advertise KHR_Debug support, but return null for this function pointer.
        /// </summary>
        public static bool HasGlObjectLabel => p_glObjectLabel != null;

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexImage2DMultisample_t(
            TextureTarget target,
            uint samples,
            PixelInternalFormat internalformat,
            uint width,
            uint height,
            GLboolean fixedsamplelocations);
        private static glTexImage2DMultisample_t p_glTexImage2DMultisample;
        public static void glTexImage2DMultiSample(
            TextureTarget target,
            uint samples,
            PixelInternalFormat internalformat,
            uint width,
            uint height,
            GLboolean fixedsamplelocations) => p_glTexImage2DMultisample(
                target,
                samples,
                internalformat,
                width,
                height,
                fixedsamplelocations);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexImage3DMultisample_t(
            TextureTarget target,
            uint samples,
            PixelInternalFormat internalformat,
            uint width,
            uint height,
            uint depth,
            GLboolean fixedsamplelocations);
        private static glTexImage3DMultisample_t p_glTexImage3DMultisample;
        public static void glTexImage3DMultisample(
            TextureTarget target,
            uint samples,
            PixelInternalFormat internalformat,
            uint width,
            uint height,
            uint depth,
            GLboolean fixedsamplelocations) => p_glTexImage3DMultisample(
                target,
                samples,
                internalformat,
                width,
                height,
                depth,
                fixedsamplelocations);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBlitFramebuffer_t(
            int srcX0,
            int srcY0,
            int srcX1,
            int srcY1,
            int dstX0,
            int dstY0,
            int dstX1,
            int dstY1,
            ClearBufferMask mask,
            BlitFramebufferFilter filter);
        private static glBlitFramebuffer_t p_glBlitFramebuffer;
        public static void glBlitFramebuffer(
            int srcX0,
            int srcY0,
            int srcX1,
            int srcY1,
            int dstX0,
            int dstY0,
            int dstX1,
            int dstY1,
            ClearBufferMask mask,
            BlitFramebufferFilter filter)
            => p_glBlitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glFramebufferTextureLayer_t(
            FramebufferTarget target,
            GLFramebufferAttachment attachment,
            uint texture,
            int level,
            int layer);
        private static glFramebufferTextureLayer_t p_glFramebufferTextureLayer;
        public static void glFramebufferTextureLayer(
            FramebufferTarget target,
            GLFramebufferAttachment attachment,
            uint texture,
            int level,
            int layer) => p_glFramebufferTextureLayer(target, attachment, texture, level, layer);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDispatchCompute_t(uint num_groups_x, uint num_groups_y, uint num_groups_z);
        private static glDispatchCompute_t p_glDispatchCompute;
        public static void glDispatchCompute(uint num_groups_x, uint num_groups_y, uint num_groups_z)
            => p_glDispatchCompute(num_groups_x, num_groups_y, num_groups_z);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate uint glGetProgramResourceIndex_t(uint program, ProgramInterface programInterface, byte* name);
        private static glGetProgramResourceIndex_t p_glGetProgramResourceIndex;
        public static uint glGetProgramResourceIndex(uint program, ProgramInterface programInterface, byte* name)
            => p_glGetProgramResourceIndex(program, programInterface, name);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glShaderStorageBlockBinding_t(uint program, uint storageBlockIndex, uint storageBlockBinding);
        private static glShaderStorageBlockBinding_t p_glShaderStorageBlockBinding;
        public static void glShaderStorageBlockBinding(uint program, uint storageBlockIndex, uint storageBlockBinding)
            => p_glShaderStorageBlockBinding(program, storageBlockIndex, storageBlockBinding);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDrawElementsIndirect_t(PrimitiveType mode, DrawElementsType type, IntPtr indirect);
        private static glDrawElementsIndirect_t p_glDrawElementsIndirect;
        public static void glDrawElementsIndirect(PrimitiveType mode, DrawElementsType type, IntPtr indirect)
            => p_glDrawElementsIndirect(mode, type, indirect);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glMultiDrawElementsIndirect_t(
            PrimitiveType mode,
            DrawElementsType type,
            IntPtr indirect,
            uint drawcount,
            uint stride);
        private static glMultiDrawElementsIndirect_t p_glMultiDrawElementsIndirect;
        public static void glMultiDrawElementsIndirect(
            PrimitiveType mode,
            DrawElementsType type,
            IntPtr indirect,
            uint drawcount,
            uint stride) => p_glMultiDrawElementsIndirect(mode, type, indirect, drawcount, stride);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDrawArraysIndirect_t(PrimitiveType mode, IntPtr indirect);
        private static glDrawArraysIndirect_t p_glDrawArraysIndirect;
        public static void glDrawArraysIndirect(PrimitiveType mode, IntPtr indirect)
            => p_glDrawArraysIndirect(mode, indirect);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glMultiDrawArraysIndirect_t(PrimitiveType mode, IntPtr indirect, uint drawcount, uint stride);
        private static glMultiDrawArraysIndirect_t p_glMultiDrawArraysIndirect;
        public static void glMultiDrawArraysIndirect(PrimitiveType mode, IntPtr indirect, uint drawcount, uint stride)
            => p_glMultiDrawArraysIndirect(mode, indirect, drawcount, stride);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDispatchComputeIndirect_t(IntPtr indirect);
        private static glDispatchComputeIndirect_t p_glDispatchComputeIndirect;
        public static void glDispatchComputeIndirect(IntPtr indirect) => p_glDispatchComputeIndirect(indirect);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBindImageTexture_t(
            uint unit​,
            uint texture​,
            int level​,
            GLboolean layered​,
            int layer​,
            TextureAccess access​,
            SizedInternalFormat format​);
        private static glBindImageTexture_t p_glBindImageTexture;
        public static void glBindImageTexture(
            uint unit​,
            uint texture​,
            int level​,
            GLboolean layered​,
            int layer​,
            TextureAccess access​,
            SizedInternalFormat format​) => p_glBindImageTexture(unit, texture, level, layered, layer, access, format);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glMemoryBarrier_t(MemoryBarrierFlags barriers);
        private static glMemoryBarrier_t p_glMemoryBarrier;
        public static void glMemoryBarrier(MemoryBarrierFlags barriers) => p_glMemoryBarrier(barriers);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexStorage1D_t(
            TextureTarget target,
            uint levels,
            SizedInternalFormat internalformat,
            uint width);
        private static glTexStorage1D_t p_glTexStorage1D;
        public static void glTexStorage1D(TextureTarget target, uint levels, SizedInternalFormat internalformat, uint width)
            => p_glTexStorage1D(target, levels, internalformat, width);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexStorage2D_t(
            TextureTarget target,
            uint levels,
            SizedInternalFormat internalformat,
            uint width,
            uint height);
        private static glTexStorage2D_t p_glTexStorage2D;
        public static void glTexStorage2D(
            TextureTarget target,
            uint levels,
            SizedInternalFormat internalformat,
            uint width,
            uint height) => p_glTexStorage2D(target, levels, internalformat, width, height);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexStorage3D_t(
            TextureTarget target,
            uint levels,
            SizedInternalFormat internalformat,
            uint width,
            uint height,
            uint depth);
        private static glTexStorage3D_t p_glTexStorage3D;
        public static void glTexStorage3D(
            TextureTarget target,
            uint levels,
            SizedInternalFormat internalformat,
            uint width,
            uint height,
            uint depth) => p_glTexStorage3D(target, levels, internalformat, width, height, depth);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTextureStorage1D_t(
            uint texture,
            uint levels,
            SizedInternalFormat internalformat,
            uint width);
        private static glTextureStorage1D_t p_glTextureStorage1D;
        public static void glTextureStorage1D(uint texture, uint levels, SizedInternalFormat internalformat, uint width)
            => p_glTextureStorage1D(texture, levels, internalformat, width);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTextureStorage2D_t(
            uint texture,
            uint levels,
            SizedInternalFormat internalformat,
            uint width,
            uint height);
        private static glTextureStorage2D_t p_glTextureStorage2D;
        public static void glTextureStorage2D(
            uint texture,
            uint levels,
            SizedInternalFormat internalformat,
            uint width,
            uint height) => p_glTextureStorage2D(texture, levels, internalformat, width, height);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTextureStorage3D_t(
            uint texture,
            uint levels,
            SizedInternalFormat internalformat,
            uint width,
            uint height,
            uint depth);
        private static glTextureStorage3D_t p_glTextureStorage3D;
        public static void glTextureStorage3D(
            uint texture,
            uint levels,
            SizedInternalFormat internalformat,
            uint width,
            uint height,
            uint depth) => p_glTextureStorage3D(texture, levels, internalformat, width, height, depth);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTextureStorage2DMultisample_t(
            uint texture,
            uint samples,
            SizedInternalFormat internalformat,
            uint width,
            uint height,
            GLboolean fixedsamplelocations);
        private static glTextureStorage2DMultisample_t p_glTextureStorage2DMultisample;
        public static void glTextureStorage2DMultisample(
            uint texture,
            uint samples,
            SizedInternalFormat internalformat,
            uint width,
            uint height,
            GLboolean fixedsamplelocations)
            => p_glTextureStorage2DMultisample(texture, samples, internalformat, width, height, fixedsamplelocations);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTextureStorage3DMultisample_t(
            uint texture,
            uint samples,
            SizedInternalFormat internalformat,
            uint width,
            uint height,
            uint depth,
            GLboolean fixedsamplelocations);
        private static glTextureStorage3DMultisample_t p_glTextureStorage3DMultisample;
        public static void glTextureStorage3DMultisample(
            uint texture,
            uint samples,
            SizedInternalFormat internalformat,
            uint width,
            uint height,
            uint depth,
            GLboolean fixedsamplelocations)
            => p_glTextureStorage3DMultisample(texture, samples, internalformat, width, height, depth, fixedsamplelocations);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexStorage2DMultisample_t(
            TextureTarget target,
            uint samples,
            SizedInternalFormat internalformat,
            uint width,
            uint height,
            GLboolean fixedsamplelocations);
        private static glTexStorage2DMultisample_t p_glTexStorage2DMultisample;
        public static void glTexStorage2DMultisample(
            TextureTarget target,
            uint samples,
            SizedInternalFormat internalformat,
            uint width,
            uint height,
            GLboolean fixedsamplelocations)
            => p_glTexStorage2DMultisample(target, samples, internalformat, width, height, fixedsamplelocations);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTexStorage3DMultisample_t(
            TextureTarget target,
            uint samples,
            SizedInternalFormat internalformat,
            uint width,
            uint height,
            uint depth,
            GLboolean fixedsamplelocations);
        private static glTexStorage3DMultisample_t p_glTexStorage3DMultisample;
        public static void glTexStorage3DMultisample(
            TextureTarget target,
            uint samples,
            SizedInternalFormat internalformat,
            uint width,
            uint height,
            uint depth,
            GLboolean fixedsamplelocations)
            => p_glTexStorage3DMultisample(target, samples, internalformat, width, height, depth, fixedsamplelocations);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glTextureView_t(
            uint texture,
            TextureTarget target,
            uint origtexture,
            PixelInternalFormat internalformat,
            uint minlevel,
            uint numlevels,
            uint minlayer,
            uint numlayers);
        private static glTextureView_t p_glTextureView;
        public static void glTextureView(
            uint texture,
            TextureTarget target,
            uint origtexture,
            PixelInternalFormat internalformat,
            uint minlevel,
            uint numlevels,
            uint minlayer,
            uint numlayers)
                => p_glTextureView(texture, target, origtexture, internalformat, minlevel, numlevels, minlayer, numlayers);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void* glMapBuffer_t(BufferTarget target, BufferAccess access);
        private static glMapBuffer_t p_glMapBuffer;
        public static void* glMapBuffer(BufferTarget target, BufferAccess access) => p_glMapBuffer(target, access);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void* glMapNamedBuffer_t(uint buffer, BufferAccess access);
        private static glMapNamedBuffer_t p_glMapNamedBuffer;
        public static void* glMapNamedBuffer(uint buffer, BufferAccess access) => p_glMapNamedBuffer(buffer, access);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate GLboolean glUnmapBuffer_t(BufferTarget target);
        private static glUnmapBuffer_t p_glUnmapBuffer;
        public static GLboolean glUnmapBuffer(BufferTarget target) => p_glUnmapBuffer(target);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate GLboolean glUnmapNamedBuffer_t(uint buffer);
        private static glUnmapNamedBuffer_t p_glUnmapNamedBuffer;
        public static GLboolean glUnmapNamedBuffer(uint buffer) => p_glUnmapNamedBuffer(buffer);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glCopyBufferSubData_t(
            BufferTarget readTarget,
            BufferTarget writeTarget,
            IntPtr readOffset,
            IntPtr writeOffset,
            IntPtr size);
        private static glCopyBufferSubData_t p_glCopyBufferSubData;
        public static void glCopyBufferSubData(
            BufferTarget readTarget,
            BufferTarget writeTarget,
            IntPtr readOffset,
            IntPtr writeOffset,
            IntPtr size) => p_glCopyBufferSubData(readTarget, writeTarget, readOffset, writeOffset, size);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glCopyTexSubImage2D_t(
            TextureTarget target,
            int level,
            int xoffset,
            int yoffset,
            int x,
            int y,
            uint width,
            uint height);
        private static glCopyTexSubImage2D_t p_glCopyTexSubImage2D;
        public static void glCopyTexSubImage2D(
            TextureTarget target,
            int level,
            int xoffset,
            int yoffset,
            int x,
            int y,
            uint width,
            uint height) => p_glCopyTexSubImage2D(target, level, xoffset, yoffset, x, y, width, height);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glCopyTexSubImage3D_t(
            TextureTarget target,
            int level,
            int xoffset,
            int yoffset,
            int zoffset,
            int x,
            int y,
            uint width,
            uint height);
        private static glCopyTexSubImage3D_t p_glCopyTexSubImage3D;
        public static void glCopyTexSubImage3D(
            TextureTarget target,
            int level,
            int xoffset,
            int yoffset,
            int zoffset,
            int x,
            int y,
            uint width,
            uint height) => p_glCopyTexSubImage3D(target, level, xoffset, yoffset, zoffset, x, y, width, height);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void* glMapBufferRange_t(BufferTarget target, IntPtr offset, IntPtr length, BufferAccessMask access);
        private static glMapBufferRange_t p_glMapBufferRange;
        public static void* glMapBufferRange(BufferTarget target, IntPtr offset, IntPtr length, BufferAccessMask access)
            => p_glMapBufferRange(target, offset, length, access);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void* glMapNamedBufferRange_t(uint buffer, IntPtr offset, uint length, BufferAccessMask access);
        private static glMapNamedBufferRange_t p_glMapNamedBufferRange;
        public static void* glMapNamedBufferRange(uint buffer, IntPtr offset, uint length, BufferAccessMask access)
            => p_glMapNamedBufferRange(buffer, offset, length, access);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetTexImage_t(
            TextureTarget target,
            int level,
            GLPixelFormat format,
            GLPixelType type,
            void* pixels);
        private static glGetTexImage_t p_glGetTexImage;
        public static void glGetTexImage(TextureTarget target, int level, GLPixelFormat format, GLPixelType type, void* pixels)
            => p_glGetTexImage(target, level, format, type, pixels);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetTextureSubImage_t(
            uint texture,
            int level,
            int xoffset,
            int yoffset,
            int zoffset,
            uint width,
            uint height,
            uint depth,
            GLPixelFormat format,
            GLPixelType type,
            uint bufSize,
            void* pixels);
        private static glGetTextureSubImage_t p_glGetTextureSubImage;
        public static void glGetTextureSubImage(
            uint texture,
            int level,
            int xoffset,
            int yoffset,
            int zoffset,
            uint width,
            uint height,
            uint depth,
            GLPixelFormat format,
            GLPixelType type,
            uint bufSize,
            void* pixels)
            => p_glGetTextureSubImage(
                texture,
                level,
                xoffset,
                yoffset,
                zoffset,
                width,
                height,
                depth,
                format,
                type,
                bufSize,
                pixels);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glCopyNamedBufferSubData_t(
            uint readBuffer,
            uint writeBuffer,
            IntPtr readOffset,
            IntPtr writeOffset,
            uint size);
        private static glCopyNamedBufferSubData_t p_glCopyNamedBufferSubData;
        public static void glCopyNamedBufferSubData(
            uint readBuffer,
            uint writeBuffer,
            IntPtr readOffset,
            IntPtr writeOffset,
            uint size) => p_glCopyNamedBufferSubData(readBuffer, writeBuffer, readOffset, writeOffset, size);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glCreateBuffers_t(uint n, uint* buffers);
        private static glCreateBuffers_t p_glCreateBuffers;
        public static void glCreateBuffers(uint n, uint* buffers) => p_glCreateBuffers(n, buffers);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glCreateTextures_t(TextureTarget target, uint n, uint* textures);
        private static glCreateTextures_t p_glCreateTextures;
        public static void glCreateTextures(TextureTarget target, uint n, uint* textures)
            => p_glCreateTextures(target, n, textures);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glCompressedTexSubImage1D_t(
            TextureTarget target,
            int level,
            int xoffset,
            uint width,
            PixelInternalFormat internalformat,
            uint imageSize,
            void* data);
        private static glCompressedTexSubImage1D_t p_glCompressedTexSubImage1D;
        public static void glCompressedTexSubImage1D(
            TextureTarget target,
            int level,
            int xoffset,
            uint width,
            PixelInternalFormat internalformat,
            uint imageSize,
            void* data) => p_glCompressedTexSubImage1D(target, level, xoffset, width, internalformat, imageSize, data);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glCompressedTexSubImage2D_t(
            TextureTarget target,
            int level,
            int xoffset,
            int yoffset,
            uint width,
            uint height,
            PixelInternalFormat format,
            uint imageSize,
            void* data);
        private static glCompressedTexSubImage2D_t p_glCompressedTexSubImage2D;
        public static void glCompressedTexSubImage2D(
            TextureTarget target,
            int level,
            int xoffset,
            int yoffset,
            uint width,
            uint height,
            PixelInternalFormat format,
            uint imageSize,
            void* data) => p_glCompressedTexSubImage2D(target, level, xoffset, yoffset, width, height, format, imageSize, data);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glCompressedTexSubImage3D_t(
            TextureTarget target,
             int level,
             int xoffset,
             int yoffset,
             int zoffset,
             uint width,
             uint height,
             uint depth,
             PixelInternalFormat format,
             uint imageSize,
             void* data);
        private static glCompressedTexSubImage3D_t p_glCompressedTexSubImage3D;
        public static void glCompressedTexSubImage3D(
            TextureTarget target,
             int level,
             int xoffset,
             int yoffset,
             int zoffset,
             uint width,
             uint height,
             uint depth,
             PixelInternalFormat format,
             uint imageSize,
             void* data)
            => p_glCompressedTexSubImage3D(
                target,
                level,
                xoffset,
                yoffset,
                zoffset,
                width,
                height,
                depth,
                format,
                imageSize,
                data);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glCopyImageSubData_t(
            uint srcName,
            TextureTarget srcTarget,
            int srcLevel,
            int srcX,
            int srcY,
            int srcZ,
            uint dstName,
            TextureTarget dstTarget,
            int dstLevel,
            int dstX,
            int dstY,
            int dstZ,
            uint srcWidth,
            uint srcHeight,
            uint srcDepth);
        private static glCopyImageSubData_t p_glCopyImageSubData;
        public static void glCopyImageSubData(
            uint srcName,
            TextureTarget srcTarget,
            int srcLevel,
            int srcX,
            int srcY,
            int srcZ,
            uint dstName,
            TextureTarget dstTarget,
            int dstLevel,
            int dstX,
            int dstY,
            int dstZ,
            uint srcWidth,
            uint srcHeight,
            uint srcDepth) => p_glCopyImageSubData(
                srcName, srcTarget,
                srcLevel, srcX, srcY, srcZ,
                dstName, dstTarget,
                dstLevel, dstX, dstY, dstZ,
                srcWidth, srcHeight, srcDepth);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glStencilFuncSeparate_t(CullFaceMode face, StencilFunction func, int @ref, uint mask);
        private static glStencilFuncSeparate_t p_glStencilFuncSeparate;
        public static void glStencilFuncSeparate(CullFaceMode face, StencilFunction func, int @ref, uint mask)
            => p_glStencilFuncSeparate(face, func, @ref, mask);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glStencilOpSeparate_t(
            CullFaceMode face,
            StencilOp sfail,
            StencilOp dpfail,
            StencilOp dppass);
        private static glStencilOpSeparate_t p_glStencilOpSeparate;
        public static void glStencilOpSeparate(
            CullFaceMode face,
            StencilOp sfail,
            StencilOp dpfail,
            StencilOp dppass) => p_glStencilOpSeparate(face, sfail, dpfail, dppass);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glStencilMask_t(uint mask);
        private static glStencilMask_t p_glStencilMask;
        public static void glStencilMask(uint mask) => p_glStencilMask(mask);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glClearStencil_t(int s);
        private static glClearStencil_t p_glClearStencil;
        public static void glClearStencil(int s) => p_glClearStencil(s);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetActiveUniformBlockiv_t(
            uint program,
            uint uniformBlockIndex,
            ActiveUniformBlockParameter pname,
            int* @params);
        private static glGetActiveUniformBlockiv_t p_glGetActiveUniformBlockiv;
        public static void glGetActiveUniformBlockiv(
            uint program,
            uint uniformBlockIndex,
            ActiveUniformBlockParameter pname,
            int* @params) => p_glGetActiveUniformBlockiv(program, uniformBlockIndex, pname, @params);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetActiveUniformBlockName_t(
            uint program,
            uint uniformBlockIndex,
            uint bufSize,
            uint* length,
            byte* uniformBlockName);
        private static glGetActiveUniformBlockName_t p_glGetActiveUniformBlockName;
        public static void glGetActiveUniformBlockName(
            uint program,
            uint uniformBlockIndex,
            uint bufSize,
            uint* length,
            byte* uniformBlockName) => p_glGetActiveUniformBlockName(program, uniformBlockIndex, bufSize, length, uniformBlockName);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetActiveUniform_t(
            uint program,
            uint index,
            uint bufSize,
            uint* length,
            int* size,
            uint* type,
            byte* name);
        private static glGetActiveUniform_t p_glGetActiveUniform;
        public static void glGetActiveUniform(
            uint program,
            uint index,
            uint bufSize,
            uint* length,
            int* size,
            uint* type,
            byte* name) => p_glGetActiveUniform(program, index, bufSize, length, size, type, name);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetCompressedTexImage_t(TextureTarget target, int level, void* pixels);
        private static glGetCompressedTexImage_t p_glGetCompressedTexImage;
        public static void glGetCompressedTexImage(TextureTarget target, int level, void* pixels)
            => p_glGetCompressedTexImage(target, level, pixels);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetCompressedTextureImage_t(uint texture, int level, uint bufSize, void* pixels);
        private static glGetCompressedTextureImage_t p_glGetCompressedTextureImage;
        public static void glGetCompressedTextureImage(uint texture, int level, uint bufSize, void* pixels)
            => p_glGetCompressedTextureImage(texture, level, bufSize, pixels);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetTexLevelParameteriv_t(
            TextureTarget target,
            int level,
            GetTextureParameter pname,
            int* @params);
        private static glGetTexLevelParameteriv_t p_glGetTexLevelParameteriv;
        public static void glGetTexLevelParameteriv(
            TextureTarget target,
            int level,
            GetTextureParameter pname,
            int* @params) => p_glGetTexLevelParameteriv(target, level, pname, @params);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glFramebufferRenderbuffer_t(
            FramebufferTarget target,
            GLFramebufferAttachment attachment,
            RenderbufferTarget renderbuffertarget,
            uint renderbuffer);
        private static glFramebufferRenderbuffer_t p_glFramebufferRenderbuffer;
        public static void glFramebufferRenderbuffer(
            FramebufferTarget target,
            GLFramebufferAttachment attachment,
            RenderbufferTarget renderbuffertarget,
            uint renderbuffer)
            => p_glFramebufferRenderbuffer(target, attachment, renderbuffertarget, renderbuffer);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glRenderbufferStorage_t(
            RenderbufferTarget target,
            uint internalformat,
            uint width,
            uint height);
        private static glRenderbufferStorage_t p_glRenderbufferStorage;
        public static void glRenderbufferStorage(
            RenderbufferTarget target,
            uint internalFormat,
            uint width,
            uint height) => p_glRenderbufferStorage(target, internalFormat, width, height);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetRenderbufferParameteriv_t(
            RenderbufferTarget target,
            RenderbufferPname pname,
            out int parameters);
        private static glGetRenderbufferParameteriv_t p_glGetRenderbufferParameteriv;
        public static void glGetRenderbufferParameteriv(
            RenderbufferTarget target,
            RenderbufferPname pname,
            out int parameters) => p_glGetRenderbufferParameteriv(target, pname, out parameters);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGenRenderbuffers_t(uint count, out uint names);
        private static glGenRenderbuffers_t p_glGenRenderbuffers;
        public static void glGenRenderbuffers(uint count, out uint names)
            => p_glGenRenderbuffers(count, out names);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glBindRenderbuffer_t(RenderbufferTarget bindPoint, uint name);
        private static glBindRenderbuffer_t p_glBindRenderbuffer;
        public static void glBindRenderbuffer(RenderbufferTarget bindPoint, uint name)
            => p_glBindRenderbuffer(bindPoint, name);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGenerateMipmap_t(TextureTarget target);
        private static glGenerateMipmap_t p_glGenerateMipmap;
        public static void glGenerateMipmap(TextureTarget target) => p_glGenerateMipmap(target);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGenerateTextureMipmap_t(uint texture);
        private static glGenerateTextureMipmap_t p_glGenerateTextureMipmap;
        public static void glGenerateTextureMipmap(uint texture) => p_glGenerateTextureMipmap(texture);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glClipControl_t(ClipControlOrigin origin, ClipControlDepthRange depth);
        private static glClipControl_t p_glClipControl;
        public static void glClipControl(ClipControlOrigin origin, ClipControlDepthRange depth)
            => p_glClipControl(origin, depth);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glGetFramebufferAttachmentParameteriv_t(
            FramebufferTarget target,
            GLFramebufferAttachment attachment,
            FramebufferParameterName pname,
            int* @params);
        private static glGetFramebufferAttachmentParameteriv_t p_glGetFramebufferAttachmentParameteriv;
        public static void glGetFramebufferAttachmentParameteriv(
            FramebufferTarget target,
            GLFramebufferAttachment attachment,
            FramebufferParameterName pname,
            int* @params) => p_glGetFramebufferAttachmentParameteriv(target, attachment, pname, @params);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glFlush_t();
        private static glFlush_t p_glFlush;
        public static void glFlush() => p_glFlush();

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glFinish_t();
        private static glFinish_t p_glFinish;
        public static void glFinish() => p_glFinish();

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glPushDebugGroup_t(DebugSource source, uint id, uint length, byte* message);
        private static glPushDebugGroup_t p_glPushDebugGroup;
        public static void glPushDebugGroup(DebugSource source, uint id, uint length, byte* message)
            => p_glPushDebugGroup(source, id, length, message);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glPopDebugGroup_t();
        private static glPopDebugGroup_t p_glPopDebugGroup;
        public static void glPopDebugGroup() => p_glPopDebugGroup();

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glDebugMessageInsert_t(
            DebugSource source,
            DebugType type,
            uint id,
            DebugSeverity severity,
            uint length,
            byte* message);
        private static glDebugMessageInsert_t p_glDebugMessageInsert;
        public static void glDebugMessageInsert(
            DebugSource source,
            DebugType type,
            uint id,
            DebugSeverity severity,
            uint length,
            byte* message) => p_glDebugMessageInsert(source, type, id, severity, length, message);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glInsertEventMarker_t(uint length, byte* marker);
        private static glInsertEventMarker_t p_glInsertEventMarker;
        public static void glInsertEventMarker(uint length, byte* marker) => p_glInsertEventMarker(length, marker);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glPushGroupMarkerEXT_t(uint length, byte* marker);
        private static glPushGroupMarkerEXT_t p_glPushGroupMarker;
        public static void glPushGroupMarker(uint length, byte* marker) => p_glPushGroupMarker(length, marker);

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glPopGroupMarkerEXT_t();
        private static glPopGroupMarkerEXT_t p_glPopGroupMarker;
        public static void glPopGroupMarker() => p_glPopGroupMarker();

        [UnmanagedFunctionPointer(CallConv)]
        private delegate void glReadPixels_t(
            int x,
            int y,
            uint width,
            uint height,
            GLPixelFormat format,
            GLPixelType type,
            void* data);
        private static glReadPixels_t p_glReadPixels;
        public static void glReadPixels(
            int x,
            int y,
            uint width,
            uint height,
            GLPixelFormat format,
            GLPixelType type,
            void* data) => p_glReadPixels(x, y, width, height, format, type, data);

        public static void LoadGetString(IntPtr glContext, Func<string, IntPtr> getProcAddress)
        {
            s_getProcAddress = getProcAddress;
            LoadFunction("glGetString", out p_glGetString);
        }

        public static void LoadAllFunctions(IntPtr glContext, Func<string, IntPtr> getProcAddress, bool gles)
        {
            s_getProcAddress = getProcAddress;

            // Common functions

            LoadFunction("glCompressedTexSubImage2D", out p_glCompressedTexSubImage2D);
            LoadFunction("glCompressedTexSubImage3D", out p_glCompressedTexSubImage3D);
            LoadFunction("glStencilFuncSeparate", out p_glStencilFuncSeparate);
            LoadFunction("glStencilOpSeparate", out p_glStencilOpSeparate);
            LoadFunction("glStencilMask", out p_glStencilMask);
            LoadFunction("glClearStencil", out p_glClearStencil);
            LoadFunction("glGetActiveUniformBlockiv", out p_glGetActiveUniformBlockiv);
            LoadFunction("glGetActiveUniformBlockName", out p_glGetActiveUniformBlockName);
            LoadFunction("glGetActiveUniform", out p_glGetActiveUniform);
            LoadFunction("glGetCompressedTexImage", out p_glGetCompressedTexImage);
            LoadFunction("glGetCompressedTextureImage", out p_glGetCompressedTextureImage);
            LoadFunction("glGetTexLevelParameteriv", out p_glGetTexLevelParameteriv);
            LoadFunction("glTexImage1D", out p_glTexImage1D);
            LoadFunction("glCompressedTexImage1D", out p_glCompressedTexSubImage1D);

            LoadFunction("glGenVertexArrays", out p_glGenVertexArrays);
            LoadFunction("glGetError", out p_glGetError);
            LoadFunction("glBindVertexArray", out p_glBindVertexArray);
            LoadFunction("glClearColor", out p_glClearColor);
            LoadFunction("glDrawBuffer", out p_glDrawBuffer);
            LoadFunction("glDrawBuffers", out p_glDrawBuffers);
            LoadFunction("glClear", out p_glClear);
            LoadFunction("glClearDepth", out p_glClearDepth);
            LoadFunction("glClearDepthf", out p_glClearDepthf);
            if (p_glClearDepthf != null) { p_glClearDepthf_Compat = p_glClearDepthf; }
            else { p_glClearDepthf_Compat = depth => p_glClearDepth(depth); }

            LoadFunction("glDrawElements", out p_glDrawElements);
            LoadFunction("glDrawElementsBaseVertex", out p_glDrawElementsBaseVertex);
            LoadFunction("glDrawElementsInstanced", out p_glDrawElementsInstanced);
            LoadFunction("glDrawElementsInstancedBaseVertex", out p_glDrawElementsInstancedBaseVertex);
            LoadFunction("glDrawArrays", out p_glDrawArrays);
            LoadFunction("glDrawArraysInstanced", out p_glDrawArraysInstanced);
            LoadFunction("glDrawArraysInstancedBaseInstance", out p_glDrawArraysInstancedBaseInstance);
            LoadFunction("glGenBuffers", out p_glGenBuffers);
            LoadFunction("glDeleteBuffers", out p_glDeleteBuffers);
            LoadFunction("glGenFramebuffers", out p_glGenFramebuffers);
            LoadFunction("glActiveTexture", out p_glActiveTexture);
            LoadFunction("glFramebufferTexture2D", out p_glFramebufferTexture2D);
            LoadFunction("glBindTexture", out p_glBindTexture);
            LoadFunction("glBindFramebuffer", out p_glBindFramebuffer);
            LoadFunction("glDeleteFramebuffers", out p_glDeleteFramebuffers);
            LoadFunction("glGenTextures", out p_glGenTextures);
            LoadFunction("glDeleteTextures", out p_glDeleteTextures);
            LoadFunction("glCheckFramebufferStatus", out p_glCheckFramebufferStatus);
            LoadFunction("glBindBuffer", out p_glBindBuffer);
            LoadFunction("glDepthRangeIndexed", out p_glDepthRangeIndexed);
            LoadFunction("glBufferSubData", out p_glBufferSubData);
            LoadFunction("glNamedBufferSubData", out p_glNamedBufferSubData);
            LoadFunction("glScissorIndexed", out p_glScissorIndexed);
            LoadFunction("glTexSubImage1D", out p_glTexSubImage1D);
            LoadFunction("glTexSubImage2D", out p_glTexSubImage2D);
            LoadFunction("glTexSubImage3D", out p_glTexSubImage3D);
            LoadFunction("glPixelStorei", out p_glPixelStorei);
            LoadFunction("glShaderSource", out p_glShaderSource);
            LoadFunction("glCreateShader", out p_glCreateShader);
            LoadFunction("glCompileShader", out p_glCompileShader);
            LoadFunction("glGetShaderiv", out p_glGetShaderiv);
            LoadFunction("glGetShaderInfoLog", out p_glGetShaderInfoLog);
            LoadFunction("glDeleteShader", out p_glDeleteShader);
            LoadFunction("glGenSamplers", out p_glGenSamplers);
            LoadFunction("glSamplerParameterf", out p_glSamplerParameterf);
            LoadFunction("glSamplerParameteri", out p_glSamplerParameteri);
            LoadFunction("glSamplerParameterfv", out p_glSamplerParameterfv);
            LoadFunction("glBindSampler", out p_glBindSampler);
            LoadFunction("glDeleteSamplers", out p_glDeleteSamplers);
            LoadFunction("glBlendFuncSeparatei", out p_glBlendFuncSeparatei);
            LoadFunction("glBlendFuncSeparate", out p_glBlendFuncSeparate);
            LoadFunction("glEnable", out p_glEnable);
            LoadFunction("glEnablei", out p_glEnablei);
            LoadFunction("glDisable", out p_glDisable);
            LoadFunction("glDisablei", out p_glDisablei);
            LoadFunction("glBlendEquationSeparatei", out p_glBlendEquationSeparatei);
            LoadFunction("glBlendEquationSeparate", out p_glBlendEquationSeparate);
            LoadFunction("glBlendColor", out p_glBlendColor);
            LoadFunction("glDepthFunc", out p_glDepthFunc);
            LoadFunction("glDepthMask", out p_glDepthMask);
            LoadFunction("glCullFace", out p_glCullFace);
            LoadFunction("glCreateProgram", out p_glCreateProgram);
            LoadFunction("glAttachShader", out p_glAttachShader);
            LoadFunction("glBindAttribLocation", out p_glBindAttribLocation);
            LoadFunction("glLinkProgram", out p_glLinkProgram);
            LoadFunction("glGetProgramiv", out p_glGetProgramiv);
            LoadFunction("glGetProgramInfoLog", out p_glGetProgramInfoLog);
            LoadFunction("glUniformBlockBinding", out p_glUniformBlockBinding);
            LoadFunction("glDeleteProgram", out p_glDeleteProgram);
            LoadFunction("glUniform1i", out p_glUniform1i);
            LoadFunction("glGetUniformBlockIndex", out p_glGetUniformBlockIndex);
            LoadFunction("glGetUniformLocation", out p_glGetUniformLocation);
            LoadFunction("glGetAttribLocation", out p_glGetAttribLocation);
            LoadFunction("glUseProgram", out p_glUseProgram);
            LoadFunction("glBindBufferRange", out p_glBindBufferRange);
            LoadFunction("glDebugMessageCallback", out p_glDebugMessageCallback);
            LoadFunction("glBufferData", out p_glBufferData);
            LoadFunction("glNamedBufferData", out p_glNamedBufferData);
            LoadFunction("glTexImage2D", out p_glTexImage2D);
            LoadFunction("glTexImage3D", out p_glTexImage3D);
            LoadFunction("glEnableVertexAttribArray", out p_glEnableVertexAttribArray);
            LoadFunction("glDisableVertexAttribArray", out p_glDisableVertexAttribArray);
            LoadFunction("glVertexAttribPointer", out p_glVertexAttribPointer);
            LoadFunction("glVertexAttribIPointer", out p_glVertexAttribIPointer);
            LoadFunction("glVertexAttribDivisor", out p_glVertexAttribDivisor);
            LoadFunction("glFrontFace", out p_glFrontFace);
            LoadFunction("glGetIntegerv", out p_glGetIntegerv);
            LoadFunction("glBindTextureUnit", out p_glBindTextureUnit);
            LoadFunction("glTexParameteri", out p_glTexParameteri);
            LoadFunction("glGetStringi", out p_glGetStringi);
            LoadFunction("glObjectLabel", out p_glObjectLabel);
            LoadFunction("glTexImage2DMultisample", out p_glTexImage2DMultisample);
            LoadFunction("glTexImage3DMultisample", out p_glTexImage3DMultisample);
            LoadFunction("glBlitFramebuffer", out p_glBlitFramebuffer);
            LoadFunction("glFramebufferTextureLayer", out p_glFramebufferTextureLayer);
            LoadFunction("glDispatchCompute", out p_glDispatchCompute);
            LoadFunction("glGetProgramResourceIndex", out p_glGetProgramResourceIndex);
            LoadFunction("glShaderStorageBlockBinding", out p_glShaderStorageBlockBinding);
            LoadFunction("glDrawElementsIndirect", out p_glDrawElementsIndirect);
            LoadFunction("glMultiDrawElementsIndirect", out p_glMultiDrawElementsIndirect);
            LoadFunction("glDrawArraysIndirect", out p_glDrawArraysIndirect);
            LoadFunction("glMultiDrawArraysIndirect", out p_glMultiDrawArraysIndirect);
            LoadFunction("glDispatchComputeIndirect", out p_glDispatchComputeIndirect);
            LoadFunction("glBindImageTexture", out p_glBindImageTexture);
            LoadFunction("glMemoryBarrier", out p_glMemoryBarrier);
            LoadFunction("glTexStorage1D", out p_glTexStorage1D);
            LoadFunction("glTexStorage2D", out p_glTexStorage2D);
            LoadFunction("glTexStorage3D", out p_glTexStorage3D);
            LoadFunction("glTextureStorage1D", out p_glTextureStorage1D);
            LoadFunction("glTextureStorage2D", out p_glTextureStorage2D);
            LoadFunction("glTextureStorage3D", out p_glTextureStorage3D);
            LoadFunction("glTextureStorage2DMultisample", out p_glTextureStorage2DMultisample);
            LoadFunction("glTextureStorage3DMultisample", out p_glTextureStorage3DMultisample);
            LoadFunction("glTexStorage2DMultisample", out p_glTexStorage2DMultisample);
            LoadFunction("glTexStorage3DMultisample", out p_glTexStorage3DMultisample);
            LoadFunction("glTextureView", out p_glTextureView);
            LoadFunction("glMapBuffer", out p_glMapBuffer);
            LoadFunction("glMapNamedBuffer", out p_glMapNamedBuffer);
            LoadFunction("glUnmapBuffer", out p_glUnmapBuffer);
            LoadFunction("glUnmapNamedBuffer", out p_glUnmapNamedBuffer);
            LoadFunction("glCopyBufferSubData", out p_glCopyBufferSubData);
            LoadFunction("glCopyTexSubImage2D", out p_glCopyTexSubImage2D);
            LoadFunction("glCopyTexSubImage3D", out p_glCopyTexSubImage3D);
            LoadFunction("glMapBufferRange", out p_glMapBufferRange);
            LoadFunction("glMapNamedBufferRange", out p_glMapNamedBufferRange);
            LoadFunction("glGetTextureSubImage", out p_glGetTextureSubImage);
            LoadFunction("glCopyNamedBufferSubData", out p_glCopyNamedBufferSubData);
            LoadFunction("glCreateBuffers", out p_glCreateBuffers);
            LoadFunction("glCreateTextures", out p_glCreateTextures);
            LoadFunction("glGenerateMipmap", out p_glGenerateMipmap);
            LoadFunction("glGetFramebufferAttachmentParameteriv", out p_glGetFramebufferAttachmentParameteriv);
            LoadFunction("glFlush", out p_glFlush);
            LoadFunction("glFinish", out p_glFinish);

            LoadFunction("glPushDebugGroup", out p_glPushDebugGroup);
            LoadFunction("glPopDebugGroup", out p_glPopDebugGroup);
            LoadFunction("glDebugMessageInsert", out p_glDebugMessageInsert);

            LoadFunction("glReadPixels", out p_glReadPixels);
            LoadFunction("glPolygonOffset", out p_glPolygonOffset);

            if (!gles)
            {
                LoadFunction("glFramebufferTexture1D", out p_glFramebufferTexture1D);
                LoadFunction("glGetTexImage", out p_glGetTexImage);
                LoadFunction("glPolygonMode", out p_glPolygonMode);
                LoadFunction("glViewportIndexedf", out p_glViewportIndexedf);
                LoadFunction("glCopyImageSubData", out p_glCopyImageSubData);
                LoadFunction("glGenerateTextureMipmap", out p_glGenerateTextureMipmap);
                LoadFunction("glClipControl", out p_glClipControl);
                LoadFunction("glDrawElementsInstancedBaseVertexBaseInstance", out p_glDrawElementsInstancedBaseVertexBaseInstance);
            }
            else
            {
                LoadFunction("glViewport", out p_glViewport);
                LoadFunction("glDepthRangef", out p_glDepthRangef);
                LoadFunction("glScissor", out p_glScissor);
                LoadFunction("glCopyImageSubData", out p_glCopyImageSubData);
                if (p_glCopyImageSubData == null)
                {
                    LoadFunction("glCopyImageSubDataOES", out p_glCopyImageSubData);
                }
                if (p_glCopyImageSubData == null)
                {
                    LoadFunction("glCopyImageSubDataEXT", out p_glCopyImageSubData);
                }

                LoadFunction("glRenderbufferStorage", out p_glRenderbufferStorage);
                LoadFunction("glFramebufferRenderbuffer", out p_glFramebufferRenderbuffer);
                LoadFunction("glGetRenderbufferParameteriv", out p_glGetRenderbufferParameteriv);
                LoadFunction("glGenRenderbuffers", out p_glGenRenderbuffers);
                LoadFunction("glBindRenderbuffer", out p_glBindRenderbuffer);
                LoadFunction("glInsertEventMarker", out p_glInsertEventMarker);
                LoadFunction("glPushGroupMarker", out p_glPushGroupMarker);
                LoadFunction("glPopGroupMarker", out p_glPopGroupMarker);
            }
        }

        private static void LoadFunction<T>(string name, out T field)
        {
            IntPtr funcPtr = s_getProcAddress(name);
            if (funcPtr != IntPtr.Zero)
            {
                field = Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
            }
            else
            {
                field = default(T);
            }
        }

        private static void LoadFunction<T>(out T field)
        {
            // Slow version using reflection -- prefer above.
            string name = typeof(T).Name;
            name = name.Substring(0, name.Length - 2); // Remove _t
            LoadFunction(name, out field);
        }
    }
}
