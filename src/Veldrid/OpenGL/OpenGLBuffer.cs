using System;
using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System.Diagnostics;

namespace Veldrid.OpenGL
{
    internal unsafe class OpenGLBuffer : DeviceBuffer, OpenGLDeferredResource
    {
        private readonly OpenGLGraphicsDevice _gd;
        private uint _buffer;
        private bool _disposeRequested;

        private string _name;
        private bool _nameChanged;

        public override string Name { get => _name; set { _name = value; _nameChanged = true; } }

        public override uint SizeInBytes { get; }
        public override BufferUsage Usage { get; }

        public uint Buffer => _buffer;

        public bool Created { get; private set; }

        public override bool IsDisposed => _disposeRequested;

        public OpenGLBuffer(OpenGLGraphicsDevice gd, uint sizeInBytes, BufferUsage usage)
        {
            _gd = gd;
            SizeInBytes = sizeInBytes;
            Usage = usage;
        }

        public void EnsureResourcesCreated()
        {
            if (!Created)
            {
                CreateGLResources();
            }
            if (_nameChanged)
            {
                _nameChanged = false;
                if (_gd.Extensions.KHR_Debug)
                {
                    SetObjectLabel(ObjectLabelIdentifier.Buffer, _buffer, _name);
                }
            }
        }

        public void CreateGLResources()
        {
            Debug.Assert(!Created);

            BufferUsageHint hint;
            if ((Usage & BufferUsage.Staging) == BufferUsage.Staging)
            {
                hint = BufferUsageHint.StreamCopy;
            }
            else if ((Usage & BufferUsage.Dynamic) == BufferUsage.Dynamic)
            {
                hint = BufferUsageHint.DynamicDraw;
            }
            else
            {
                hint = BufferUsageHint.StaticDraw;
            }

            if (_gd.Extensions.ARB_DirectStateAccess)
            {
                uint buffer;
                glCreateBuffers(1, &buffer);
                CheckLastError();
                _buffer = buffer;

                glNamedBufferData(
                    _buffer,
                    SizeInBytes,
                    null,
                    hint);
                CheckLastError();
            }
            else
            {
                glGenBuffers(1, out _buffer);
                CheckLastError();

                glBindBuffer(BufferTarget.CopyReadBuffer, _buffer);
                CheckLastError();

                glBufferData(
                    BufferTarget.CopyReadBuffer,
                    (UIntPtr)SizeInBytes,
                    null,
                    hint);
                CheckLastError();
            }

            Created = true;
        }

        public override void Dispose()
        {
            if (!_disposeRequested)
            {
                _disposeRequested = true;
                _gd.EnqueueDisposal(this);
            }
        }

        public void DestroyGLResources()
        {
            uint buffer = _buffer;
            glDeleteBuffers(1, ref buffer);
            CheckLastError();
        }
    }
}
