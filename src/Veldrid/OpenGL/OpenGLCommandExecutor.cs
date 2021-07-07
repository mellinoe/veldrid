using System;
using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System.Text;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLCommandExecutor
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly GraphicsBackend _backend;
        private readonly OpenGLTextureSamplerManager _textureSamplerManager;
        private readonly StagingMemoryPool _stagingMemoryPool;
        private readonly OpenGLExtensions _extensions;
        private readonly OpenGLPlatformInfo _platformInfo;
        private readonly GraphicsDeviceFeatures _features;

        private Framebuffer _fb;
        private bool _isSwapchainFB;
        private OpenGLPipeline _graphicsPipeline;
        private BoundResourceSetInfo[] _graphicsResourceSets = Array.Empty<BoundResourceSetInfo>();
        private bool[] _newGraphicsResourceSets = Array.Empty<bool>();
        private OpenGLBuffer[] _vertexBuffers = Array.Empty<OpenGLBuffer>();
        private uint[] _vbOffsets = Array.Empty<uint>();
        private uint[] _vertexAttribDivisors = Array.Empty<uint>();
        private uint _vertexAttributesBound;
        private readonly Viewport[] _viewports = new Viewport[20];
        private DrawElementsType _drawElementsType;
        private uint _ibOffset;
        private PrimitiveType _primitiveType;

        private OpenGLPipeline _computePipeline;
        private BoundResourceSetInfo[] _computeResourceSets = Array.Empty<BoundResourceSetInfo>();
        private bool[] _newComputeResourceSets = Array.Empty<bool>();

        private bool _graphicsPipelineActive;
        private bool _vertexLayoutFlushed;

        public OpenGLCommandExecutor(OpenGLGraphicsDevice gd, OpenGLPlatformInfo platformInfo)
        {
            _gd = gd;
            _backend = gd.BackendType;
            _extensions = gd.Extensions;
            _textureSamplerManager = gd.TextureSamplerManager;
            _stagingMemoryPool = gd.StagingMemoryPool;
            _platformInfo = platformInfo;
            _features = gd.Features;
        }

        public void Begin()
        {
        }

        public void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            if (!_isSwapchainFB)
            {
                DrawBuffersEnum bufs = (DrawBuffersEnum)((uint)DrawBuffersEnum.ColorAttachment0 + index);
                glDrawBuffers(1, &bufs);
                CheckLastError();
            }

            RgbaFloat color = clearColor;
            glClearColor(color.R, color.G, color.B, color.A);
            CheckLastError();

            if (_graphicsPipeline != null && _graphicsPipeline.RasterizerState.ScissorTestEnabled)
            {
                glDisable(EnableCap.ScissorTest);
                CheckLastError();
            }

            glClear(ClearBufferMask.ColorBufferBit);
            CheckLastError();

            if (_graphicsPipeline != null && _graphicsPipeline.RasterizerState.ScissorTestEnabled)
            {
                glEnable(EnableCap.ScissorTest);
            }

            if (!_isSwapchainFB)
            {
                int colorCount = _fb.ColorTargets.Count;
                DrawBuffersEnum* bufs = stackalloc DrawBuffersEnum[colorCount];
                for (int i = 0; i < colorCount; i++)
                {
                    bufs[i] = DrawBuffersEnum.ColorAttachment0 + i;
                }
                glDrawBuffers((uint)colorCount, bufs);
                CheckLastError();
            }
        }

        public void ClearDepthStencil(float depth, byte stencil)
        {
            glClearDepth_Compat(depth);
            CheckLastError();

            glClearStencil(stencil);
            CheckLastError();

            if (_graphicsPipeline != null && _graphicsPipeline.RasterizerState.ScissorTestEnabled)
            {
                glDisable(EnableCap.ScissorTest);
                CheckLastError();
            }

            glDepthMask(true);
            glClear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            CheckLastError();

            if (_graphicsPipeline != null && _graphicsPipeline.RasterizerState.ScissorTestEnabled)
            {
                glEnable(EnableCap.ScissorTest);
            }
        }

        public void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            PreDrawCommand();

            if (instanceCount == 1 && instanceStart == 0)
            {
                glDrawArrays(_primitiveType, (int)vertexStart, vertexCount);
                CheckLastError();
            }
            else
            {
                if (instanceStart == 0)
                {
                    glDrawArraysInstanced(_primitiveType, (int)vertexStart, vertexCount, instanceCount);
                    CheckLastError();
                }
                else
                {
                    glDrawArraysInstancedBaseInstance(_primitiveType, (int)vertexStart, vertexCount, instanceCount, instanceStart);
                    CheckLastError();
                }
            }
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            PreDrawCommand();

            uint indexSize = _drawElementsType == DrawElementsType.UnsignedShort ? 2u : 4u;
            void* indices = (void*)((indexStart * indexSize) + _ibOffset);

            if (instanceCount == 1 && instanceStart == 0)
            {
                if (vertexOffset == 0)
                {
                    glDrawElements(_primitiveType, indexCount, _drawElementsType, indices);
                    CheckLastError();
                }
                else
                {
                    glDrawElementsBaseVertex(_primitiveType, indexCount, _drawElementsType, indices, vertexOffset);
                    CheckLastError();
                }
            }
            else
            {
                if (instanceStart > 0)
                {
                    glDrawElementsInstancedBaseVertexBaseInstance(
                        _primitiveType,
                        indexCount,
                        _drawElementsType,
                        indices,
                        instanceCount,
                        vertexOffset,
                        instanceStart);
                    CheckLastError();
                }
                else if (vertexOffset == 0)
                {
                    glDrawElementsInstanced(_primitiveType, indexCount, _drawElementsType, indices, instanceCount);
                    CheckLastError();
                }
                else
                {
                    glDrawElementsInstancedBaseVertex(
                        _primitiveType,
                        indexCount,
                        _drawElementsType,
                        indices,
                        instanceCount,
                        vertexOffset);
                    CheckLastError();
                }
            }
        }

        public void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();

            OpenGLBuffer glBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(indirectBuffer);
            glBindBuffer(BufferTarget.DrawIndirectBuffer, glBuffer.Buffer);
            CheckLastError();

            if (_extensions.MultiDrawIndirect)
            {
                glMultiDrawArraysIndirect(_primitiveType, (IntPtr)offset, drawCount, stride);
                CheckLastError();
            }
            else
            {
                uint indirect = offset;
                for (uint i = 0; i < drawCount; i++)
                {
                    glDrawArraysIndirect(_primitiveType, (IntPtr)indirect);
                    CheckLastError();

                    indirect += stride;
                }
            }
        }

        public void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            PreDrawCommand();

            OpenGLBuffer glBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(indirectBuffer);
            glBindBuffer(BufferTarget.DrawIndirectBuffer, glBuffer.Buffer);
            CheckLastError();

            if (_extensions.MultiDrawIndirect)
            {
                glMultiDrawElementsIndirect(_primitiveType, _drawElementsType, (IntPtr)offset, drawCount, stride);
                CheckLastError();
            }
            else
            {
                uint indirect = offset;
                for (uint i = 0; i < drawCount; i++)
                {
                    glDrawElementsIndirect(_primitiveType, _drawElementsType, (IntPtr)indirect);
                    CheckLastError();

                    indirect += stride;
                }
            }
        }

        private void PreDrawCommand()
        {
            if (!_graphicsPipelineActive)
            {
                ActivateGraphicsPipeline();
            }

            FlushResourceSets(graphics: true);
            if (!_vertexLayoutFlushed)
            {
                FlushVertexLayouts();
                _vertexLayoutFlushed = true;
            }
        }

        private void FlushResourceSets(bool graphics)
        {
            uint sets = graphics
                ? (uint)_graphicsPipeline.ResourceLayouts.Length
                : (uint)_computePipeline.ResourceLayouts.Length;
            for (uint slot = 0; slot < sets; slot++)
            {
                BoundResourceSetInfo brsi = graphics ? _graphicsResourceSets[slot] : _computeResourceSets[slot];
                OpenGLResourceSet glSet = Util.AssertSubtype<ResourceSet, OpenGLResourceSet>(brsi.Set);
                ResourceLayoutElementDescription[] layoutElements = glSet.Layout.Elements;
                bool isNew = graphics ? _newGraphicsResourceSets[slot] : _newComputeResourceSets[slot];

                ActivateResourceSet(slot, graphics, brsi, layoutElements, isNew);
            }

            Util.ClearArray(graphics ? _newGraphicsResourceSets : _newComputeResourceSets);
        }

        private void FlushVertexLayouts()
        {
            uint totalSlotsBound = 0;
            VertexLayoutDescription[] layouts = _graphicsPipeline.VertexLayouts;
            for (int i = 0; i < layouts.Length; i++)
            {
                VertexLayoutDescription input = layouts[i];
                OpenGLBuffer vb = _vertexBuffers[i];
                glBindBuffer(BufferTarget.ArrayBuffer, vb.Buffer);
                uint offset = 0;
                uint vbOffset = _vbOffsets[i];
                for (uint slot = 0; slot < input.Elements.Length; slot++)
                {
                    ref VertexElementDescription element = ref input.Elements[slot]; // Large structure -- use by reference.
                    uint actualSlot = totalSlotsBound + slot;
                    if (actualSlot >= _vertexAttributesBound)
                    {
                        glEnableVertexAttribArray(actualSlot);
                    }
                    VertexAttribPointerType type = OpenGLFormats.VdToGLVertexAttribPointerType(
                        element.Format,
                        out bool normalized,
                        out bool isInteger);

                    uint actualOffset = element.Offset != 0 ? element.Offset : offset;
                    actualOffset += vbOffset;

                    if (isInteger && !normalized)
                    {
                        glVertexAttribIPointer(
                            actualSlot,
                            FormatHelpers.GetElementCount(element.Format),
                            type,
                            (uint)_graphicsPipeline.VertexStrides[i],
                            (void*)actualOffset);
                        CheckLastError();
                    }
                    else
                    {
                        glVertexAttribPointer(
                            actualSlot,
                            FormatHelpers.GetElementCount(element.Format),
                            type,
                            normalized,
                            (uint)_graphicsPipeline.VertexStrides[i],
                            (void*)actualOffset);
                        CheckLastError();
                    }

                    uint stepRate = input.InstanceStepRate;
                    if (_vertexAttribDivisors[actualSlot] != stepRate)
                    {
                        glVertexAttribDivisor(actualSlot, stepRate);
                        _vertexAttribDivisors[actualSlot] = stepRate;
                    }

                    offset += FormatHelpers.GetSizeInBytes(element.Format);
                }

                totalSlotsBound += (uint)input.Elements.Length;
            }

            for (uint extraSlot = totalSlotsBound; extraSlot < _vertexAttributesBound; extraSlot++)
            {
                glDisableVertexAttribArray(extraSlot);
            }

            _vertexAttributesBound = totalSlotsBound;
        }

        internal void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            PreDispatchCommand();

            glDispatchCompute(groupCountX, groupCountY, groupCountZ);
            CheckLastError();

            PostDispatchCommand();
        }

        public void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset)
        {
            PreDispatchCommand();

            OpenGLBuffer glBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(indirectBuffer);
            glBindBuffer(BufferTarget.DrawIndirectBuffer, glBuffer.Buffer);
            CheckLastError();

            glDispatchComputeIndirect((IntPtr)offset);
            CheckLastError();

            PostDispatchCommand();
        }

        private void PreDispatchCommand()
        {
            if (_graphicsPipelineActive)
            {
                ActivateComputePipeline();
            }

            FlushResourceSets(false);
        }

        private static void PostDispatchCommand()
        {
            // TODO: Smart barriers?
            glMemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            CheckLastError();
        }

        public void End()
        {
        }

        public void SetFramebuffer(Framebuffer fb)
        {
            if (fb is OpenGLFramebuffer glFB)
            {
                if (_backend == GraphicsBackend.OpenGL || _extensions.EXT_sRGBWriteControl)
                {
                    glEnable(EnableCap.FramebufferSrgb);
                    CheckLastError();
                }

                glFB.EnsureResourcesCreated();
                glBindFramebuffer(FramebufferTarget.Framebuffer, glFB.Framebuffer);
                CheckLastError();
                _isSwapchainFB = false;
            }
            else if (fb is OpenGLSwapchainFramebuffer swapchainFB)
            {
                if ((_backend == GraphicsBackend.OpenGL || _extensions.EXT_sRGBWriteControl))
                {
                    if (swapchainFB.DisableSrgbConversion)
                    {
                        glDisable(EnableCap.FramebufferSrgb);
                        CheckLastError();
                    }
                    else
                    {
                        glEnable(EnableCap.FramebufferSrgb);
                        CheckLastError();
                    }
                }

                if (_platformInfo.SetSwapchainFramebuffer != null)
                {
                    _platformInfo.SetSwapchainFramebuffer();
                }
                else
                {
                    glBindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    CheckLastError();
                }

                _isSwapchainFB = true;
            }
            else
            {
                throw new VeldridException("Invalid Framebuffer type: " + fb.GetType().Name);
            }

            _fb = fb;
        }

        public void SetIndexBuffer(DeviceBuffer ib, IndexFormat format, uint offset)
        {
            OpenGLBuffer glIB = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(ib);
            glIB.EnsureResourcesCreated();

            glBindBuffer(BufferTarget.ElementArrayBuffer, glIB.Buffer);
            CheckLastError();

            _drawElementsType = OpenGLFormats.VdToGLDrawElementsType(format);
            _ibOffset = offset;
        }

        public void SetPipeline(Pipeline pipeline)
        {
            if (!pipeline.IsComputePipeline && _graphicsPipeline != pipeline)
            {
                _graphicsPipeline = Util.AssertSubtype<Pipeline, OpenGLPipeline>(pipeline);
                ActivateGraphicsPipeline();
                _vertexLayoutFlushed = false;
            }
            else if (pipeline.IsComputePipeline && _computePipeline != pipeline)
            {
                _computePipeline = Util.AssertSubtype<Pipeline, OpenGLPipeline>(pipeline);
                ActivateComputePipeline();
                _vertexLayoutFlushed = false;
            }
        }

        private void ActivateGraphicsPipeline()
        {
            _graphicsPipelineActive = true;
            _graphicsPipeline.EnsureResourcesCreated();

            Util.EnsureArrayMinimumSize(ref _graphicsResourceSets, (uint)_graphicsPipeline.ResourceLayouts.Length);
            Util.EnsureArrayMinimumSize(ref _newGraphicsResourceSets, (uint)_graphicsPipeline.ResourceLayouts.Length);

            // Force ResourceSets to be re-bound.
            for (int i = 0; i < _graphicsPipeline.ResourceLayouts.Length; i++)
            {
                _newGraphicsResourceSets[i] = true;
            }

            // Blend State

            BlendStateDescription blendState = _graphicsPipeline.BlendState;
            glBlendColor(blendState.BlendFactor.R, blendState.BlendFactor.G, blendState.BlendFactor.B, blendState.BlendFactor.A);
            CheckLastError();

            if (blendState.AlphaToCoverageEnabled)
            {
                glEnable(EnableCap.SampleAlphaToCoverage);
                CheckLastError();
            }
            else
            {
                glDisable(EnableCap.SampleAlphaToCoverage);
                CheckLastError();
            }

            if (_features.IndependentBlend)
            {
                for (uint i = 0; i < blendState.AttachmentStates.Length; i++)
                {
                    BlendAttachmentDescription attachment = blendState.AttachmentStates[i];
                    if (!attachment.BlendEnabled)
                    {
                        glDisablei(EnableCap.Blend, i);
                        CheckLastError();
                    }
                    else
                    {
                        glEnablei(EnableCap.Blend, i);
                        CheckLastError();

                        glBlendFuncSeparatei(
                            i,
                            OpenGLFormats.VdToGLBlendFactorSrc(attachment.SourceColorFactor),
                            OpenGLFormats.VdToGLBlendFactorDest(attachment.DestinationColorFactor),
                            OpenGLFormats.VdToGLBlendFactorSrc(attachment.SourceAlphaFactor),
                            OpenGLFormats.VdToGLBlendFactorDest(attachment.DestinationAlphaFactor));
                        CheckLastError();

                        glBlendEquationSeparatei(
                            i,
                            OpenGLFormats.VdToGLBlendEquationMode(attachment.ColorFunction),
                            OpenGLFormats.VdToGLBlendEquationMode(attachment.AlphaFunction));
                        CheckLastError();
                    }
                }
            }
            else if (blendState.AttachmentStates.Length > 0)
            {
                BlendAttachmentDescription attachment = blendState.AttachmentStates[0];
                if (!attachment.BlendEnabled)
                {
                    glDisable(EnableCap.Blend);
                    CheckLastError();
                }
                else
                {
                    glEnable(EnableCap.Blend);
                    CheckLastError();

                    glBlendFuncSeparate(
                        OpenGLFormats.VdToGLBlendFactorSrc(attachment.SourceColorFactor),
                        OpenGLFormats.VdToGLBlendFactorDest(attachment.DestinationColorFactor),
                        OpenGLFormats.VdToGLBlendFactorSrc(attachment.SourceAlphaFactor),
                        OpenGLFormats.VdToGLBlendFactorDest(attachment.DestinationAlphaFactor));
                    CheckLastError();

                    glBlendEquationSeparate(
                        OpenGLFormats.VdToGLBlendEquationMode(attachment.ColorFunction),
                        OpenGLFormats.VdToGLBlendEquationMode(attachment.AlphaFunction));
                    CheckLastError();
                }
            }

            // Depth Stencil State

            DepthStencilStateDescription dss = _graphicsPipeline.DepthStencilState;
            if (!dss.DepthTestEnabled)
            {
                glDisable(EnableCap.DepthTest);
                CheckLastError();
            }
            else
            {
                glEnable(EnableCap.DepthTest);
                CheckLastError();

                glDepthFunc(OpenGLFormats.VdToGLDepthFunction(dss.DepthComparison));
                CheckLastError();
            }

            glDepthMask(dss.DepthWriteEnabled);
            CheckLastError();

            if (dss.StencilTestEnabled)
            {
                glEnable(EnableCap.StencilTest);
                CheckLastError();

                glStencilFuncSeparate(
                    CullFaceMode.Front,
                    OpenGLFormats.VdToGLStencilFunction(dss.StencilFront.Comparison),
                    (int)dss.StencilReference,
                    dss.StencilReadMask);
                CheckLastError();

                glStencilOpSeparate(
                    CullFaceMode.Front,
                    OpenGLFormats.VdToGLStencilOp(dss.StencilFront.Fail),
                    OpenGLFormats.VdToGLStencilOp(dss.StencilFront.DepthFail),
                    OpenGLFormats.VdToGLStencilOp(dss.StencilFront.Pass));
                CheckLastError();

                glStencilFuncSeparate(
                    CullFaceMode.Back,
                    OpenGLFormats.VdToGLStencilFunction(dss.StencilBack.Comparison),
                    (int)dss.StencilReference,
                    dss.StencilReadMask);
                CheckLastError();

                glStencilOpSeparate(
                    CullFaceMode.Back,
                    OpenGLFormats.VdToGLStencilOp(dss.StencilBack.Fail),
                    OpenGLFormats.VdToGLStencilOp(dss.StencilBack.DepthFail),
                    OpenGLFormats.VdToGLStencilOp(dss.StencilBack.Pass));
                CheckLastError();

                glStencilMask(dss.StencilWriteMask);
                CheckLastError();
            }
            else
            {
                glDisable(EnableCap.StencilTest);
                CheckLastError();
            }

            // Rasterizer State

            RasterizerStateDescription rs = _graphicsPipeline.RasterizerState;
            if (rs.CullMode == FaceCullMode.None)
            {
                glDisable(EnableCap.CullFace);
                CheckLastError();
            }
            else
            {
                glEnable(EnableCap.CullFace);
                CheckLastError();

                glCullFace(OpenGLFormats.VdToGLCullFaceMode(rs.CullMode));
                CheckLastError();
            }

            if (_backend == GraphicsBackend.OpenGL)
            {
                glPolygonMode(MaterialFace.FrontAndBack, OpenGLFormats.VdToGLPolygonMode(rs.FillMode));
                CheckLastError();
            }

            if (!rs.ScissorTestEnabled)
            {
                glDisable(EnableCap.ScissorTest);
                CheckLastError();
            }
            else
            {
                glEnable(EnableCap.ScissorTest);
                CheckLastError();
            }

            if (_backend == GraphicsBackend.OpenGL)
            {
                if (!rs.DepthClipEnabled)
                {
                    glEnable(EnableCap.DepthClamp);
                    CheckLastError();
                }
                else
                {
                    glDisable(EnableCap.DepthClamp);
                    CheckLastError();
                }
            }

            glFrontFace(OpenGLFormats.VdToGLFrontFaceDirection(rs.FrontFace));
            CheckLastError();

            if (rs.DepthBiasEnabled)
            {
                glEnable(EnableCap.PolygonOffsetFill);
                CheckLastError();

                glEnable(EnableCap.PolygonOffsetLine);
                CheckLastError();

                glEnable(EnableCap.PolygonOffsetPoint);
                CheckLastError();

                glPolygonOffset(rs.DepthBiasSlopeScaled, rs.DepthBiasConstant);
                CheckLastError();
            }
            else
            {
                glDisable(EnableCap.PolygonOffsetFill);
                CheckLastError();

                glDisable(EnableCap.PolygonOffsetLine);
                CheckLastError();

                glDisable(EnableCap.PolygonOffsetPoint);
                CheckLastError();

                glPolygonOffset(0, 0);
                CheckLastError();
            }

            // Primitive Topology
            _primitiveType = OpenGLFormats.VdToGLPrimitiveType(_graphicsPipeline.PrimitiveTopology);

            // Shader Set
            glUseProgram(_graphicsPipeline.Program);
            CheckLastError();

            int vertexStridesCount = _graphicsPipeline.VertexStrides.Length;
            Util.EnsureArrayMinimumSize(ref _vertexBuffers, (uint)vertexStridesCount);
            Util.EnsureArrayMinimumSize(ref _vbOffsets, (uint)vertexStridesCount);

            uint totalVertexElements = 0;
            for (int i = 0; i < _graphicsPipeline.VertexLayouts.Length; i++)
            {
                totalVertexElements += (uint)_graphicsPipeline.VertexLayouts[i].Elements.Length;
            }
            Util.EnsureArrayMinimumSize(ref _vertexAttribDivisors, totalVertexElements);
        }

        public void GenerateMipmaps(Texture texture)
        {
            OpenGLTexture glTex = Util.AssertSubtype<Texture, OpenGLTexture>(texture);
            glTex.EnsureResourcesCreated();
            if (_extensions.ARB_DirectStateAccess)
            {
                glGenerateTextureMipmap(glTex.Texture);
                CheckLastError();
            }
            else
            {
                TextureTarget target = glTex.TextureTarget;
                _textureSamplerManager.SetTextureTransient(target, glTex.Texture);
                glGenerateMipmap(target);
                CheckLastError();
            }
        }

        public void PushDebugGroup(string name)
        {
            if (_extensions.KHR_Debug)
            {
                int byteCount = Encoding.UTF8.GetByteCount(name);
                byte* utf8Ptr = stackalloc byte[byteCount];
                fixed (char* namePtr = name)
                {
                    Encoding.UTF8.GetBytes(namePtr, name.Length, utf8Ptr, byteCount);
                }
                glPushDebugGroup(DebugSource.DebugSourceApplication, 0, (uint)byteCount, utf8Ptr);
                CheckLastError();
            }
            else if (_extensions.EXT_DebugMarker)
            {
                int byteCount = Encoding.UTF8.GetByteCount(name);
                byte* utf8Ptr = stackalloc byte[byteCount];
                fixed (char* namePtr = name)
                {
                    Encoding.UTF8.GetBytes(namePtr, name.Length, utf8Ptr, byteCount);
                }
                glPushGroupMarker((uint)byteCount, utf8Ptr);
                CheckLastError();
            }
        }

        public void PopDebugGroup()
        {
            if (_extensions.KHR_Debug)
            {
                glPopDebugGroup();
                CheckLastError();
            }
            else if (_extensions.EXT_DebugMarker)
            {
                glPopGroupMarker();
                CheckLastError();
            }
        }

        public void InsertDebugMarker(string name)
        {
            if (_extensions.KHR_Debug)
            {
                int byteCount = Encoding.UTF8.GetByteCount(name);
                byte* utf8Ptr = stackalloc byte[byteCount];
                fixed (char* namePtr = name)
                {
                    Encoding.UTF8.GetBytes(namePtr, name.Length, utf8Ptr, byteCount);
                }

                glDebugMessageInsert(
                    DebugSource.DebugSourceApplication,
                    DebugType.DebugTypeMarker,
                    0,
                    DebugSeverity.DebugSeverityNotification,
                    (uint)byteCount,
                    utf8Ptr);
                CheckLastError();
            }
            else if (_extensions.EXT_DebugMarker)
            {
                int byteCount = Encoding.UTF8.GetByteCount(name);
                byte* utf8Ptr = stackalloc byte[byteCount];
                fixed (char* namePtr = name)
                {
                    Encoding.UTF8.GetBytes(namePtr, name.Length, utf8Ptr, byteCount);
                }

                glInsertEventMarker((uint)byteCount, utf8Ptr);
                CheckLastError();
            }
        }

        private void ActivateComputePipeline()
        {
            _graphicsPipelineActive = false;
            _computePipeline.EnsureResourcesCreated();
            Util.EnsureArrayMinimumSize(ref _computeResourceSets, (uint)_computePipeline.ResourceLayouts.Length);
            Util.EnsureArrayMinimumSize(ref _newComputeResourceSets, (uint)_computePipeline.ResourceLayouts.Length);

            // Force ResourceSets to be re-bound.
            for (int i = 0; i < _computePipeline.ResourceLayouts.Length; i++)
            {
                _newComputeResourceSets[i] = true;
            }

            // Shader Set
            glUseProgram(_computePipeline.Program);
            CheckLastError();
        }

        public void SetGraphicsResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets)
        {
            if (!_graphicsResourceSets[slot].Equals(rs, dynamicOffsetCount, ref dynamicOffsets))
            {
                _graphicsResourceSets[slot].Offsets.Dispose();
                _graphicsResourceSets[slot] = new BoundResourceSetInfo(rs, dynamicOffsetCount, ref dynamicOffsets);
                _newGraphicsResourceSets[slot] = true;
            }
        }

        public void SetComputeResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets)
        {
            if (!_computeResourceSets[slot].Equals(rs, dynamicOffsetCount, ref dynamicOffsets))
            {
                _computeResourceSets[slot].Offsets.Dispose();
                _computeResourceSets[slot] = new BoundResourceSetInfo(rs, dynamicOffsetCount, ref dynamicOffsets);
                _newComputeResourceSets[slot] = true;
            }
        }

        private void ActivateResourceSet(
            uint slot,
            bool graphics,
            BoundResourceSetInfo brsi,
            ResourceLayoutElementDescription[] layoutElements,
            bool isNew)
        {
            OpenGLResourceSet glResourceSet = Util.AssertSubtype<ResourceSet, OpenGLResourceSet>(brsi.Set);
            OpenGLPipeline pipeline = graphics ? _graphicsPipeline : _computePipeline;
            uint ubBaseIndex = GetUniformBaseIndex(slot, graphics);
            uint ssboBaseIndex = GetShaderStorageBaseIndex(slot, graphics);

            uint ubOffset = 0;
            uint ssboOffset = 0;
            uint dynamicOffsetIndex = 0;
            for (uint element = 0; element < glResourceSet.Resources.Length; element++)
            {
                ResourceKind kind = layoutElements[element].Kind;
                BindableResource resource = glResourceSet.Resources[(int)element];

                uint bufferOffset = 0;
                if (glResourceSet.Layout.IsDynamicBuffer(element))
                {
                    bufferOffset = brsi.Offsets.Get(dynamicOffsetIndex);
                    dynamicOffsetIndex += 1;
                }

                switch (kind)
                {
                    case ResourceKind.UniformBuffer:
                    {
                        if (!isNew) { continue; }

                        DeviceBufferRange range = Util.GetBufferRange(resource, bufferOffset);
                        OpenGLBuffer glUB = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(range.Buffer);

                        glUB.EnsureResourcesCreated();
                        if (pipeline.GetUniformBindingForSlot(slot, element, out OpenGLUniformBinding uniformBindingInfo))
                        {
                            if (range.SizeInBytes < uniformBindingInfo.BlockSize)
                            {
                                string name = glResourceSet.Layout.Elements[element].Name;
                                throw new VeldridException(
                                    $"Not enough data in uniform buffer \"{name}\" (slot {slot}, element {element}). Shader expects at least {uniformBindingInfo.BlockSize} bytes, but buffer only contains {range.SizeInBytes} bytes");
                            }
                            glUniformBlockBinding(pipeline.Program, uniformBindingInfo.BlockLocation, ubBaseIndex + ubOffset);
                            CheckLastError();

                            glBindBufferRange(
                                BufferRangeTarget.UniformBuffer,
                                ubBaseIndex + ubOffset,
                                glUB.Buffer,
                                (IntPtr)range.Offset,
                                (UIntPtr)range.SizeInBytes);
                            CheckLastError();

                            ubOffset += 1;
                        }
                        break;
                    }
                    case ResourceKind.StructuredBufferReadWrite:
                    case ResourceKind.StructuredBufferReadOnly:
                    {
                        if (!isNew) { continue; }

                        DeviceBufferRange range = Util.GetBufferRange(resource, bufferOffset);
                        OpenGLBuffer glBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(range.Buffer);

                        glBuffer.EnsureResourcesCreated();
                        if (pipeline.GetStorageBufferBindingForSlot(slot, element, out OpenGLShaderStorageBinding shaderStorageBinding))
                        {
                            if (_backend == GraphicsBackend.OpenGL)
                            {
                                glShaderStorageBlockBinding(
                                    pipeline.Program,
                                    shaderStorageBinding.StorageBlockBinding,
                                    ssboBaseIndex + ssboOffset);
                                CheckLastError();

                                glBindBufferRange(
                                    BufferRangeTarget.ShaderStorageBuffer,
                                    ssboBaseIndex + ssboOffset,
                                    glBuffer.Buffer,
                                    (IntPtr)range.Offset,
                                    (UIntPtr)range.SizeInBytes);
                                CheckLastError();
                            }
                            else
                            {
                                glBindBufferRange(
                                    BufferRangeTarget.ShaderStorageBuffer,
                                    shaderStorageBinding.StorageBlockBinding,
                                    glBuffer.Buffer,
                                    (IntPtr)range.Offset,
                                    (UIntPtr)range.SizeInBytes);
                                CheckLastError();
                            }
                            ssboOffset += 1;
                        }
                        break;
                    }
                    case ResourceKind.TextureReadOnly:
                        TextureView texView = Util.GetTextureView(_gd, resource);
                        OpenGLTextureView glTexView = Util.AssertSubtype<TextureView, OpenGLTextureView>(texView);
                        glTexView.EnsureResourcesCreated();
                        if (pipeline.GetTextureBindingInfo(slot, element, out OpenGLTextureBindingSlotInfo textureBindingInfo))
                        {
                            _textureSamplerManager.SetTexture((uint)textureBindingInfo.RelativeIndex, glTexView);
                            glUniform1i(textureBindingInfo.UniformLocation, textureBindingInfo.RelativeIndex);
                            CheckLastError();
                        }
                        break;
                    case ResourceKind.TextureReadWrite:
                        TextureView texViewRW = Util.GetTextureView(_gd, resource);
                        OpenGLTextureView glTexViewRW = Util.AssertSubtype<TextureView, OpenGLTextureView>(texViewRW);
                        glTexViewRW.EnsureResourcesCreated();
                        if (pipeline.GetTextureBindingInfo(slot, element, out OpenGLTextureBindingSlotInfo imageBindingInfo))
                        {
                            if (_backend == GraphicsBackend.OpenGL)
                            {
                                glBindImageTexture(
                                    (uint)imageBindingInfo.RelativeIndex,
                                    glTexViewRW.Target.Texture,
                                    0,
                                    false,
                                    0,
                                    TextureAccess.ReadWrite,
                                    glTexViewRW.GetReadWriteSizedInternalFormat());
                                CheckLastError();
                                glUniform1i(imageBindingInfo.UniformLocation, imageBindingInfo.RelativeIndex);
                                CheckLastError();
                            }
                            else
                            {
                                glBindImageTexture(
                                    (uint)imageBindingInfo.UniformLocation,
                                    glTexViewRW.Target.Texture,
                                    0,
                                    false,
                                    0,
                                    TextureAccess.ReadWrite,
                                    glTexViewRW.GetReadWriteSizedInternalFormat());
                                CheckLastError();
                            }
                        }
                        break;
                    case ResourceKind.Sampler:
                        OpenGLSampler glSampler = Util.AssertSubtype<BindableResource, OpenGLSampler>(resource);
                        glSampler.EnsureResourcesCreated();
                        if (pipeline.GetSamplerBindingInfo(slot, element, out OpenGLSamplerBindingSlotInfo samplerBindingInfo))
                        {
                            foreach (int index in samplerBindingInfo.RelativeIndices)
                            {
                                _textureSamplerManager.SetSampler((uint)index, glSampler);
                            }
                        }
                        break;
                    default: throw Illegal.Value<ResourceKind>();
                }
            }
        }

        public void ResolveTexture(Texture source, Texture destination)
        {
            OpenGLTexture glSourceTex = Util.AssertSubtype<Texture, OpenGLTexture>(source);
            OpenGLTexture glDestinationTex = Util.AssertSubtype<Texture, OpenGLTexture>(destination);
            glSourceTex.EnsureResourcesCreated();
            glDestinationTex.EnsureResourcesCreated();

            uint sourceFramebuffer = glSourceTex.GetFramebuffer(0, 0);
            uint destinationFramebuffer = glDestinationTex.GetFramebuffer(0, 0);

            glBindFramebuffer(FramebufferTarget.ReadFramebuffer, sourceFramebuffer);
            CheckLastError();

            glBindFramebuffer(FramebufferTarget.DrawFramebuffer, destinationFramebuffer);
            CheckLastError();

            glDisable(EnableCap.ScissorTest);
            CheckLastError();

            glBlitFramebuffer(
                0,
                0,
                (int)source.Width,
                (int)source.Height,
                0,
                0,
                (int)destination.Width,
                (int)destination.Height,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Nearest);
            CheckLastError();
        }

        private uint GetUniformBaseIndex(uint slot, bool graphics)
        {
            OpenGLPipeline pipeline = graphics ? _graphicsPipeline : _computePipeline;
            uint ret = 0;
            for (uint i = 0; i < slot; i++)
            {
                ret += pipeline.GetUniformBufferCount(i);
            }

            return ret;
        }

        private uint GetShaderStorageBaseIndex(uint slot, bool graphics)
        {
            OpenGLPipeline pipeline = graphics ? _graphicsPipeline : _computePipeline;
            uint ret = 0;
            for (uint i = 0; i < slot; i++)
            {
                ret += pipeline.GetShaderStorageBufferCount(i);
            }

            return ret;
        }

        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            if (_backend == GraphicsBackend.OpenGL)
            {
                glScissorIndexed(
                    index,
                    (int)x,
                    (int)(_fb.Height - (int)height - y),
                    width,
                    height);
                CheckLastError();
            }
            else
            {
                if (index == 0)
                {
                    glScissor(
                        (int)x,
                        (int)(_fb.Height - (int)height - y),
                        width,
                        height);
                    CheckLastError();
                }
            }
        }

        public void SetVertexBuffer(uint index, DeviceBuffer vb, uint offset)
        {
            OpenGLBuffer glVB = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(vb);
            glVB.EnsureResourcesCreated();

            Util.EnsureArrayMinimumSize(ref _vertexBuffers, index + 1);
            Util.EnsureArrayMinimumSize(ref _vbOffsets, index + 1);
            _vertexLayoutFlushed = false;
            _vertexBuffers[index] = glVB;
            _vbOffsets[index] = offset;
        }

        public void SetViewport(uint index, ref Viewport viewport)
        {
            _viewports[(int)index] = viewport;

            if (_backend == GraphicsBackend.OpenGL)
            {
                float left = viewport.X;
                float bottom = _fb.Height - (viewport.Y + viewport.Height);

                glViewportIndexed(index, left, bottom, viewport.Width, viewport.Height);
                CheckLastError();

                glDepthRangeIndexed(index, viewport.MinDepth, viewport.MaxDepth);
                CheckLastError();
            }
            else
            {
                if (index == 0)
                {
                    glViewport((int)viewport.X, (int)viewport.Y, (uint)viewport.Width, (uint)viewport.Height);
                    CheckLastError();

                    glDepthRangef(viewport.MinDepth, viewport.MaxDepth);
                    CheckLastError();
                }
            }
        }

        public void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr dataPtr, uint sizeInBytes)
        {
            OpenGLBuffer glBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(buffer);
            glBuffer.EnsureResourcesCreated();

            if (_extensions.ARB_DirectStateAccess)
            {
                glNamedBufferSubData(
                    glBuffer.Buffer,
                    (IntPtr)bufferOffsetInBytes,
                    sizeInBytes,
                    dataPtr.ToPointer());
                CheckLastError();
            }
            else
            {
                BufferTarget bufferTarget = BufferTarget.CopyWriteBuffer;
                glBindBuffer(bufferTarget, glBuffer.Buffer);
                CheckLastError();
                glBufferSubData(
                    bufferTarget,
                    (IntPtr)bufferOffsetInBytes,
                    (UIntPtr)sizeInBytes,
                    dataPtr.ToPointer());
                CheckLastError();
            }
        }

        public void UpdateTexture(
            Texture texture,
            IntPtr dataPtr,
            uint x,
            uint y,
            uint z,
            uint width,
            uint height,
            uint depth,
            uint mipLevel,
            uint arrayLayer)
        {
            if (width == 0 || height == 0 || depth == 0) { return; }

            OpenGLTexture glTex = Util.AssertSubtype<Texture, OpenGLTexture>(texture);
            glTex.EnsureResourcesCreated();

            TextureTarget texTarget = glTex.TextureTarget;

            _textureSamplerManager.SetTextureTransient(texTarget, glTex.Texture);
            CheckLastError();

            bool isCompressed = FormatHelpers.IsCompressedFormat(texture.Format);
            uint blockSize = isCompressed ? 4u : 1u;

            uint blockAlignedWidth = Math.Max(width, blockSize);
            uint blockAlignedHeight = Math.Max(height, blockSize);

            uint rowPitch = FormatHelpers.GetRowPitch(blockAlignedWidth, texture.Format);
            uint depthPitch = FormatHelpers.GetDepthPitch(rowPitch, blockAlignedHeight, texture.Format);

            // Compressed textures can specify regions that are larger than the dimensions.
            // We should only pass up to the dimensions to OpenGL, though.
            Util.GetMipDimensions(glTex, mipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
            width = Math.Min(width, mipWidth);
            height = Math.Min(height, mipHeight);

            uint unpackAlignment = 4;
            if (!isCompressed)
            {
                unpackAlignment = FormatHelpers.GetSizeInBytes(glTex.Format);
            }
            if (unpackAlignment < 4)
            {
                glPixelStorei(PixelStoreParameter.UnpackAlignment, (int)unpackAlignment);
                CheckLastError();
            }

            if (texTarget == TextureTarget.Texture1D)
            {
                if (isCompressed)
                {
                    glCompressedTexSubImage1D(
                        TextureTarget.Texture1D,
                        (int)mipLevel,
                        (int)x,
                        width,
                        glTex.GLInternalFormat,
                        rowPitch,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage1D(
                        TextureTarget.Texture1D,
                        (int)mipLevel,
                        (int)x,
                        width,
                        glTex.GLPixelFormat,
                        glTex.GLPixelType,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else if (texTarget == TextureTarget.Texture1DArray)
            {
                if (isCompressed)
                {
                    glCompressedTexSubImage2D(
                        TextureTarget.Texture1DArray,
                        (int)mipLevel,
                        (int)x,
                        (int)arrayLayer,
                        width,
                        1,
                        glTex.GLInternalFormat,
                        rowPitch,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage2D(
                    TextureTarget.Texture1DArray,
                    (int)mipLevel,
                    (int)x,
                    (int)arrayLayer,
                    width,
                    1,
                    glTex.GLPixelFormat,
                    glTex.GLPixelType,
                    dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else if (texTarget == TextureTarget.Texture2D)
            {
                if (isCompressed)
                {
                    glCompressedTexSubImage2D(
                        TextureTarget.Texture2D,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        width,
                        height,
                        glTex.GLInternalFormat,
                        depthPitch,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage2D(
                        TextureTarget.Texture2D,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        width,
                        height,
                        glTex.GLPixelFormat,
                        glTex.GLPixelType,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else if (texTarget == TextureTarget.Texture2DArray)
            {
                if (isCompressed)
                {
                    glCompressedTexSubImage3D(
                        TextureTarget.Texture2DArray,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        (int)arrayLayer,
                        width,
                        height,
                        1,
                        glTex.GLInternalFormat,
                        depthPitch,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage3D(
                        TextureTarget.Texture2DArray,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        (int)arrayLayer,
                        width,
                        height,
                        1,
                        glTex.GLPixelFormat,
                        glTex.GLPixelType,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else if (texTarget == TextureTarget.Texture3D)
            {
                if (isCompressed)
                {
                    glCompressedTexSubImage3D(
                        TextureTarget.Texture3D,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        (int)z,
                        width,
                        height,
                        depth,
                        glTex.GLInternalFormat,
                        depthPitch * depth,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage3D(
                        TextureTarget.Texture3D,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        (int)z,
                        width,
                        height,
                        depth,
                        glTex.GLPixelFormat,
                        glTex.GLPixelType,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else if (texTarget == TextureTarget.TextureCubeMap)
            {
                TextureTarget cubeTarget = GetCubeTarget(arrayLayer);
                if (isCompressed)
                {
                    glCompressedTexSubImage2D(
                        cubeTarget,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        width,
                        height,
                        glTex.GLInternalFormat,
                        depthPitch,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage2D(
                        cubeTarget,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        width,
                        height,
                        glTex.GLPixelFormat,
                        glTex.GLPixelType,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else if (texTarget == TextureTarget.TextureCubeMapArray)
            {
                if (isCompressed)
                {
                    glCompressedTexSubImage3D(
                        TextureTarget.TextureCubeMapArray,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        (int)arrayLayer,
                        width,
                        height,
                        1,
                        glTex.GLInternalFormat,
                        depthPitch,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
                else
                {
                    glTexSubImage3D(
                        TextureTarget.TextureCubeMapArray,
                        (int)mipLevel,
                        (int)x,
                        (int)y,
                        (int)arrayLayer,
                        width,
                        height,
                        1,
                        glTex.GLPixelFormat,
                        glTex.GLPixelType,
                        dataPtr.ToPointer());
                    CheckLastError();
                }
            }
            else
            {
                throw new VeldridException($"Invalid OpenGL TextureTarget encountered: {glTex.TextureTarget}.");
            }

            if (unpackAlignment < 4)
            {
                glPixelStorei(PixelStoreParameter.UnpackAlignment, 4);
                CheckLastError();
            }
        }

        private TextureTarget GetCubeTarget(uint arrayLayer)
        {
            switch (arrayLayer)
            {
                case 0:
                    return TextureTarget.TextureCubeMapPositiveX;
                case 1:
                    return TextureTarget.TextureCubeMapNegativeX;
                case 2:
                    return TextureTarget.TextureCubeMapPositiveY;
                case 3:
                    return TextureTarget.TextureCubeMapNegativeY;
                case 4:
                    return TextureTarget.TextureCubeMapPositiveZ;
                case 5:
                    return TextureTarget.TextureCubeMapNegativeZ;
                default:
                    throw new VeldridException("Unexpected array layer in UpdateTexture called on a cubemap texture.");
            }
        }

        public void CopyBuffer(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
            OpenGLBuffer srcGLBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(source);
            OpenGLBuffer dstGLBuffer = Util.AssertSubtype<DeviceBuffer, OpenGLBuffer>(destination);

            srcGLBuffer.EnsureResourcesCreated();
            dstGLBuffer.EnsureResourcesCreated();

            if (_extensions.ARB_DirectStateAccess)
            {
                glCopyNamedBufferSubData(
                    srcGLBuffer.Buffer,
                    dstGLBuffer.Buffer,
                    (IntPtr)sourceOffset,
                    (IntPtr)destinationOffset,
                    sizeInBytes);
            }
            else
            {
                glBindBuffer(BufferTarget.CopyReadBuffer, srcGLBuffer.Buffer);
                CheckLastError();

                glBindBuffer(BufferTarget.CopyWriteBuffer, dstGLBuffer.Buffer);
                CheckLastError();

                glCopyBufferSubData(
                    BufferTarget.CopyReadBuffer,
                    BufferTarget.CopyWriteBuffer,
                    (IntPtr)sourceOffset,
                    (IntPtr)destinationOffset,
                    (IntPtr)sizeInBytes);
                CheckLastError();
            }
        }

        public void CopyTexture(
            Texture source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel,
            uint srcBaseArrayLayer,
            Texture destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel,
            uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount)
        {
            OpenGLTexture srcGLTexture = Util.AssertSubtype<Texture, OpenGLTexture>(source);
            OpenGLTexture dstGLTexture = Util.AssertSubtype<Texture, OpenGLTexture>(destination);

            srcGLTexture.EnsureResourcesCreated();
            dstGLTexture.EnsureResourcesCreated();

            if (_extensions.CopyImage && depth == 1)
            {
                // glCopyImageSubData does not work properly when depth > 1, so use the awful roundabout copy.
                uint srcZOrLayer = Math.Max(srcBaseArrayLayer, srcZ);
                uint dstZOrLayer = Math.Max(dstBaseArrayLayer, dstZ);
                uint depthOrLayerCount = Math.Max(depth, layerCount);
                // Copy width and height are allowed to be a full compressed block size, even if the mip level only contains a
                // region smaller than the block size.
                Util.GetMipDimensions(source, srcMipLevel, out uint mipWidth, out uint mipHeight, out _);
                width = Math.Min(width, mipWidth);
                height = Math.Min(height, mipHeight);
                glCopyImageSubData(
                    srcGLTexture.Texture, srcGLTexture.TextureTarget, (int)srcMipLevel, (int)srcX, (int)srcY, (int)srcZOrLayer,
                    dstGLTexture.Texture, dstGLTexture.TextureTarget, (int)dstMipLevel, (int)dstX, (int)dstY, (int)dstZOrLayer,
                    width, height, depthOrLayerCount);
                CheckLastError();
            }
            else
            {
                for (uint layer = 0; layer < layerCount; layer++)
                {
                    uint srcLayer = layer + srcBaseArrayLayer;
                    uint dstLayer = layer + dstBaseArrayLayer;
                    CopyRoundabout(
                        srcGLTexture, dstGLTexture,
                        srcX, srcY, srcZ, srcMipLevel, srcLayer,
                        dstX, dstY, dstZ, dstMipLevel, dstLayer,
                        width, height, depth);
                }
            }
        }

        private void CopyRoundabout(
            OpenGLTexture srcGLTexture, OpenGLTexture dstGLTexture,
            uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcLayer,
            uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstLayer,
            uint width, uint height, uint depth)
        {
            bool isCompressed = FormatHelpers.IsCompressedFormat(srcGLTexture.Format);
            if (srcGLTexture.Format != dstGLTexture.Format)
            {
                throw new VeldridException("Copying to/from Textures with different formats is not supported.");
            }

            uint packAlignment = 4;
            uint depthSliceSize = 0;
            uint sizeInBytes;
            TextureTarget srcTarget = srcGLTexture.TextureTarget;
            if (isCompressed)
            {
                _textureSamplerManager.SetTextureTransient(srcTarget, srcGLTexture.Texture);
                CheckLastError();

                int compressedSize;
                glGetTexLevelParameteriv(
                    srcTarget,
                    (int)srcMipLevel,
                    GetTextureParameter.TextureCompressedImageSize,
                    &compressedSize);
                CheckLastError();
                sizeInBytes = (uint)compressedSize;
            }
            else
            {
                uint pixelSize = FormatHelpers.GetSizeInBytes(srcGLTexture.Format);
                packAlignment = pixelSize;
                depthSliceSize = width * height * pixelSize;
                sizeInBytes = depthSliceSize * depth;
            }

            StagingBlock block = _stagingMemoryPool.GetStagingBlock(sizeInBytes);

            if (packAlignment < 4)
            {
                glPixelStorei(PixelStoreParameter.PackAlignment, (int)packAlignment);
                CheckLastError();
            }

            if (isCompressed)
            {
                if (_extensions.ARB_DirectStateAccess)
                {
                    glGetCompressedTextureImage(
                        srcGLTexture.Texture,
                        (int)srcMipLevel,
                        block.SizeInBytes,
                        block.Data);
                    CheckLastError();
                }
                else
                {
                    _textureSamplerManager.SetTextureTransient(srcTarget, srcGLTexture.Texture);
                    CheckLastError();

                    glGetCompressedTexImage(srcTarget, (int)srcMipLevel, block.Data);
                    CheckLastError();
                }

                TextureTarget dstTarget = dstGLTexture.TextureTarget;
                _textureSamplerManager.SetTextureTransient(dstTarget, dstGLTexture.Texture);
                CheckLastError();

                Util.GetMipDimensions(srcGLTexture, srcMipLevel, out uint mipWidth, out uint mipHeight, out uint mipDepth);
                uint fullRowPitch = FormatHelpers.GetRowPitch(mipWidth, srcGLTexture.Format);
                uint fullDepthPitch = FormatHelpers.GetDepthPitch(
                    fullRowPitch,
                    mipHeight,
                    srcGLTexture.Format);

                uint denseRowPitch = FormatHelpers.GetRowPitch(width, srcGLTexture.Format);
                uint denseDepthPitch = FormatHelpers.GetDepthPitch(denseRowPitch, height, srcGLTexture.Format);
                uint numRows = FormatHelpers.GetNumRows(height, srcGLTexture.Format);
                uint trueCopySize = denseRowPitch * numRows;
                StagingBlock trueCopySrc = _stagingMemoryPool.GetStagingBlock(trueCopySize);

                uint layerStartOffset = denseDepthPitch * srcLayer;

                Util.CopyTextureRegion(
                    (byte*)block.Data + layerStartOffset,
                    srcX, srcY, srcZ,
                    fullRowPitch, fullDepthPitch,
                    trueCopySrc.Data,
                    0, 0, 0,
                    denseRowPitch,
                    denseDepthPitch,
                    width, height, depth,
                    srcGLTexture.Format);

                UpdateTexture(
                    dstGLTexture,
                    (IntPtr)trueCopySrc.Data,
                    dstX, dstY, dstZ,
                    width, height, 1,
                    dstMipLevel, dstLayer);

                _stagingMemoryPool.Free(trueCopySrc);
            }
            else // !isCompressed
            {
                if (_extensions.ARB_DirectStateAccess)
                {
                    glGetTextureSubImage(
                        srcGLTexture.Texture, (int)srcMipLevel, (int)srcX, (int)srcY, (int)srcZ,
                        width, height, depth,
                        srcGLTexture.GLPixelFormat, srcGLTexture.GLPixelType, block.SizeInBytes, block.Data);
                    CheckLastError();
                }
                else
                {
                    for (uint layer = 0; layer < depth; layer++)
                    {
                        uint curLayer = srcZ + srcLayer + layer;
                        uint curOffset = depthSliceSize * layer;
                        glGenFramebuffers(1, out uint readFB);
                        CheckLastError();
                        glBindFramebuffer(FramebufferTarget.ReadFramebuffer, readFB);
                        CheckLastError();

                        if (srcGLTexture.ArrayLayers > 1 || srcGLTexture.Type == TextureType.Texture3D
                            || (srcGLTexture.Usage & TextureUsage.Cubemap) != 0)
                        {
                            glFramebufferTextureLayer(
                                FramebufferTarget.ReadFramebuffer,
                                GLFramebufferAttachment.ColorAttachment0,
                                srcGLTexture.Texture,
                                (int)srcMipLevel,
                                (int)curLayer);
                            CheckLastError();
                        }
                        else if (srcGLTexture.Type == TextureType.Texture1D)
                        {
                            glFramebufferTexture1D(
                                FramebufferTarget.ReadFramebuffer,
                                GLFramebufferAttachment.ColorAttachment0,
                                TextureTarget.Texture1D,
                                srcGLTexture.Texture,
                                (int)srcMipLevel);
                            CheckLastError();
                        }
                        else
                        {
                            glFramebufferTexture2D(
                                FramebufferTarget.ReadFramebuffer,
                                GLFramebufferAttachment.ColorAttachment0,
                                TextureTarget.Texture2D,
                                srcGLTexture.Texture,
                                (int)srcMipLevel);
                            CheckLastError();
                        }

                        CheckLastError();
                        glReadPixels(
                            (int)srcX, (int)srcY,
                            width, height,
                            srcGLTexture.GLPixelFormat,
                            srcGLTexture.GLPixelType,
                            (byte*)block.Data + curOffset);
                        CheckLastError();
                        glDeleteFramebuffers(1, ref readFB);
                        CheckLastError();
                    }
                }

                UpdateTexture(
                    dstGLTexture,
                    (IntPtr)block.Data,
                    dstX, dstY, dstZ,
                    width, height, depth, dstMipLevel, dstLayer);
            }

            if (packAlignment < 4)
            {
                glPixelStorei(PixelStoreParameter.PackAlignment, 4);
                CheckLastError();
            }

            _stagingMemoryPool.Free(block);
        }

        private static void CopyWithFBO(
            OpenGLTextureSamplerManager textureSamplerManager,
            OpenGLTexture srcGLTexture, OpenGLTexture dstGLTexture,
            uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcBaseArrayLayer,
            uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstBaseArrayLayer,
            uint width, uint height, uint depth, uint layerCount, uint layer)
        {
            TextureTarget dstTarget = dstGLTexture.TextureTarget;
            if (dstTarget == TextureTarget.Texture2D)
            {
                glBindFramebuffer(
                    FramebufferTarget.ReadFramebuffer,
                    srcGLTexture.GetFramebuffer(srcMipLevel, srcBaseArrayLayer + layer));
                CheckLastError();

                textureSamplerManager.SetTextureTransient(TextureTarget.Texture2D, dstGLTexture.Texture);
                CheckLastError();

                glCopyTexSubImage2D(
                    TextureTarget.Texture2D,
                    (int)dstMipLevel,
                    (int)dstX, (int)dstY,
                    (int)srcX, (int)srcY,
                    width, height);
                CheckLastError();
            }
            else if (dstTarget == TextureTarget.Texture2DArray)
            {
                glBindFramebuffer(
                    FramebufferTarget.ReadFramebuffer,
                    srcGLTexture.GetFramebuffer(srcMipLevel, srcBaseArrayLayer + layerCount));

                textureSamplerManager.SetTextureTransient(TextureTarget.Texture2DArray, dstGLTexture.Texture);
                CheckLastError();

                glCopyTexSubImage3D(
                    TextureTarget.Texture2DArray,
                    (int)dstMipLevel,
                    (int)dstX,
                    (int)dstY,
                    (int)(dstBaseArrayLayer + layer),
                    (int)srcX,
                    (int)srcY,
                    width,
                    height);
                CheckLastError();
            }
            else if (dstTarget == TextureTarget.Texture3D)
            {
                textureSamplerManager.SetTextureTransient(TextureTarget.Texture3D, dstGLTexture.Texture);
                CheckLastError();

                for (uint i = srcZ; i < srcZ + depth; i++)
                {
                    glCopyTexSubImage3D(
                        TextureTarget.Texture3D,
                        (int)dstMipLevel,
                        (int)dstX,
                        (int)dstY,
                        (int)dstZ,
                        (int)srcX,
                        (int)srcY,
                        width,
                        height);
                }
                CheckLastError();
            }
        }
    }
}
