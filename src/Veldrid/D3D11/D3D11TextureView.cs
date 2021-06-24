using Vortice.Direct3D11;
using System;

namespace Veldrid.D3D11
{
    internal class D3D11TextureView : TextureView
    {
        private string _name;
        private bool _disposed;

        public ID3D11ShaderResourceView ShaderResourceView { get; }
        public ID3D11UnorderedAccessView UnorderedAccessView { get; }

        public D3D11TextureView(D3D11GraphicsDevice gd, ref TextureViewDescription description)
            : base(ref description)
        {
            ID3D11Device device = gd.Device;
            D3D11Texture d3dTex = Util.AssertSubtype<Texture, D3D11Texture>(description.Target);
            ShaderResourceViewDescription srvDesc = D3D11Util.GetSrvDesc(
                d3dTex,
                description.BaseMipLevel,
                description.MipLevels,
                description.BaseArrayLayer,
                description.ArrayLayers,
                Format);
            ShaderResourceView = device.CreateShaderResourceView(d3dTex.DeviceTexture, srvDesc);

            if ((d3dTex.Usage & TextureUsage.Storage) == TextureUsage.Storage)
            {
                UnorderedAccessViewDescription uavDesc = new UnorderedAccessViewDescription();
                uavDesc.Format = D3D11Formats.GetViewFormat(d3dTex.DxgiFormat);

                if ((d3dTex.Usage & TextureUsage.Cubemap) == TextureUsage.Cubemap)
                {
                    throw new NotSupportedException();
                }
                else if (d3dTex.Depth == 1)
                {
                    if (d3dTex.ArrayLayers == 1)
                    {
                        if (d3dTex.Type == TextureType.Texture1D)
                        {
                            uavDesc.ViewDimension = UnorderedAccessViewDimension.Texture1D;
                            uavDesc.Texture1D.MipSlice = (int)description.BaseMipLevel;
                        }
                        else
                        {
                            uavDesc.ViewDimension = UnorderedAccessViewDimension.Texture2D;
                            uavDesc.Texture2D.MipSlice = (int)description.BaseMipLevel;
                        }
                    }
                    else
                    {
                        if (d3dTex.Type == TextureType.Texture1D)
                        {
                            uavDesc.ViewDimension = UnorderedAccessViewDimension.Texture1DArray;
                            uavDesc.Texture1DArray.MipSlice = (int)description.BaseMipLevel;
                            uavDesc.Texture1DArray.FirstArraySlice = (int)description.BaseArrayLayer;
                            uavDesc.Texture1DArray.ArraySize = (int)description.ArrayLayers;
                        }
                        else
                        {
                            uavDesc.ViewDimension = UnorderedAccessViewDimension.Texture2DArray;
                            uavDesc.Texture2DArray.MipSlice = (int)description.BaseMipLevel;
                            uavDesc.Texture2DArray.FirstArraySlice = (int)description.BaseArrayLayer;
                            uavDesc.Texture2DArray.ArraySize = (int)description.ArrayLayers;
                        }
                    }
                }
                else
                {
                    uavDesc.ViewDimension = UnorderedAccessViewDimension.Texture3D;
                    uavDesc.Texture3D.MipSlice = (int)description.BaseMipLevel;

                    // Map the entire range of the 3D texture.
                    uavDesc.Texture3D.FirstWSlice = 0;
                    uavDesc.Texture3D.WSize = (int)d3dTex.Depth;
                }

                UnorderedAccessView = device.CreateUnorderedAccessView(d3dTex.DeviceTexture, uavDesc);
            }
        }

        public override string Name
        {
            get => _name;
            set
            {
                _name = value;
                if (ShaderResourceView != null)
                {
                    ShaderResourceView.DebugName = value + "_SRV";
                }
                if (UnorderedAccessView != null)
                {
                    UnorderedAccessView.DebugName = value + "_UAV";
                }
            }
        }

        public override bool IsDisposed => _disposed;

        public override void Dispose()
        {
            if (!_disposed)
            {
                ShaderResourceView?.Dispose();
                UnorderedAccessView?.Dispose();
                _disposed = true;
            }
        }
    }
}
