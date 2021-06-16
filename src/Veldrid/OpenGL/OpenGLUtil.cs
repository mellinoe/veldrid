using System.Diagnostics;
using System.Text;
using Veldrid.OpenGLBinding;
using static Veldrid.OpenGLBinding.OpenGLNative;

namespace Veldrid.OpenGL
{
    internal static class OpenGLUtil
    {
        private static int? MaxLabelLength;

        [Conditional("DEBUG")]
        [DebuggerNonUserCode]
        internal static void CheckLastError()
        {
            uint error = glGetError();
            if (error != 0)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw new VeldridException("glGetError indicated an error: " + (ErrorCode)error);
            }
        }

        [Conditional("DEBUG")]
        internal unsafe static void ValidateProgram(uint program)
        {
            glValidateProgram(program);

            int validateStatus;
            glGetProgramiv(program, GetProgramParameterName.ValidateStatus, &validateStatus);
            CheckLastError();
            if (validateStatus != 1)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                byte* infoLog = stackalloc byte[4096];
                uint bytesWritten;
                glGetProgramInfoLog(program, 4096, &bytesWritten, infoLog);
                CheckLastError();
                string log = Encoding.UTF8.GetString(infoLog, (int)bytesWritten);
                if(bytesWritten == 0)
                    log = "Unknown";
                throw new VeldridException($"Error validate GL program: {log}");
            }
        }

        internal static unsafe void SetObjectLabel(ObjectLabelIdentifier identifier, uint target, string name)
        {
            if (HasGlObjectLabel)
            {
                int byteCount = Encoding.UTF8.GetByteCount(name);
                if (MaxLabelLength == null)
                {
                    int maxLabelLength = -1;
                    glGetIntegerv(GetPName.MaxLabelLength, &maxLabelLength);
                    CheckLastError();
                    MaxLabelLength = maxLabelLength;
                }
                if (byteCount >= MaxLabelLength)
                {
                    name = name.Substring(0, MaxLabelLength.Value - 4) + "...";
                    byteCount = Encoding.UTF8.GetByteCount(name);
                }

                byte* utf8Ptr = stackalloc byte[byteCount];
                fixed (char* namePtr = name)
                {
                    Encoding.UTF8.GetBytes(namePtr, name.Length, utf8Ptr, byteCount);
                    glObjectLabel(identifier, target, (uint)byteCount, utf8Ptr);
                    CheckLastError();
                }
            }
        }

        internal static TextureTarget GetTextureTarget(OpenGLTexture glTex, uint arrayLayer)
        {
            if ((glTex.Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
            {
                switch (arrayLayer % 6)
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
                }
            }

            return glTex.TextureTarget;
        }
    }
}
