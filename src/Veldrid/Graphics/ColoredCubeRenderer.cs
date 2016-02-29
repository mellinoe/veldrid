﻿using System;
using System.Numerics;

namespace Veldrid.Graphics
{
    public class ColoredCubeRenderer
    {
        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Material _material;

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One;

        private DynamicDataProvider<Matrix4x4> _modelViewProvider = new DynamicDataProvider<Matrix4x4>();

        public ColoredCubeRenderer(RenderContext context)
        {
            ResourceFactory factory = context.ResourceFactory;

            _vb = factory.CreateVertexBuffer(VertexPositionColor.SizeInBytes * s_cubeVertices.Length);
            VertexDescriptor desc = new VertexDescriptor(VertexPositionColor.SizeInBytes, VertexPositionColor.ElementCount, 0, IntPtr.Zero);
            _vb.SetVertexData(s_cubeVertices, desc);

            _ib = factory.CreateIndexBuffer(sizeof(int) * s_cubeIndices.Length);
            _ib.SetIndices(s_cubeIndices, 0, IntPtr.Zero);

            MaterialVertexInput materialInputs = new MaterialVertexInput(
                VertexPositionColor.SizeInBytes,
                new MaterialVertexInputElement[]
                {
                    new MaterialVertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3),
                    new MaterialVertexInputElement("in_color", VertexSemanticType.Color, VertexElementFormat.Float4)
                });

            MaterialGlobalInputs globalInputs = new MaterialGlobalInputs(
                new MaterialGlobalInputElement[]
                {
                    new MaterialGlobalInputElement("projectionMatrixUniform", MaterialGlobalInputType.Matrix4x4, context.ProjectionMatrixProvider),
                    new MaterialGlobalInputElement("modelviewMatrixUniform", MaterialGlobalInputType.Matrix4x4, _modelViewProvider),
                });

            _material = factory.CreateMaterial(VertexShaderSource, FragmentShaderSource, materialInputs, globalInputs, MaterialTextureInputs.Empty);
        }

        public unsafe void Render(RenderContext context)
        {
            float rotationAmount = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            _modelViewProvider.Data =
                Matrix4x4.CreateScale(Scale)
                * Matrix4x4.CreateRotationY(rotationAmount)
                * Matrix4x4.CreateTranslation(Position)
                * context.ViewMatrixProvider.Data;

            context.SetVertexBuffer(_vb);
            context.SetIndexBuffer(_ib);
            context.SetMaterial(_material);

            context.DrawIndexedPrimitives(0, s_cubeIndices.Length);
        }

        private static readonly VertexPositionColor[] s_cubeVertices = new VertexPositionColor[]
        {
            // Top
            new VertexPositionColor(new Vector3(-.5f,.5f,-.5f),    RgbaFloat.Red),
            new VertexPositionColor(new Vector3(.5f,.5f,-.5f),     RgbaFloat.Red),
            new VertexPositionColor(new Vector3(.5f,.5f,.5f),      RgbaFloat.Red),
            new VertexPositionColor(new Vector3(-.5f,.5f,.5f),     RgbaFloat.Red),
            // Bottom
            new VertexPositionColor(new Vector3(-.5f,-.5f,.5f),    RgbaFloat.Grey),
            new VertexPositionColor(new Vector3(.5f,-.5f,.5f),     RgbaFloat.Grey),
            new VertexPositionColor(new Vector3(.5f,-.5f,-.5f),    RgbaFloat.Grey),
            new VertexPositionColor(new Vector3(-.5f,-.5f,-.5f),   RgbaFloat.Grey),
            // Left
            new VertexPositionColor(new Vector3(-.5f,.5f,-.5f),    RgbaFloat.Blue),
            new VertexPositionColor(new Vector3(-.5f,.5f,.5f),     RgbaFloat.Blue),
            new VertexPositionColor(new Vector3(-.5f,-.5f,.5f),    RgbaFloat.Blue),
            new VertexPositionColor(new Vector3(-.5f,-.5f,-.5f),   RgbaFloat.Blue),
            // Right
            new VertexPositionColor(new Vector3(.5f,.5f,.5f),      RgbaFloat.White),
            new VertexPositionColor(new Vector3(.5f,.5f,-.5f),     RgbaFloat.White),
            new VertexPositionColor(new Vector3(.5f,-.5f,-.5f),    RgbaFloat.White),
            new VertexPositionColor(new Vector3(.5f,-.5f,.5f),     RgbaFloat.White),
            // Back
            new VertexPositionColor(new Vector3(.5f,.5f,-.5f),     RgbaFloat.Yellow),
            new VertexPositionColor(new Vector3(-.5f,.5f,-.5f),    RgbaFloat.Yellow),
            new VertexPositionColor(new Vector3(-.5f,-.5f,-.5f),   RgbaFloat.Yellow),
            new VertexPositionColor(new Vector3(.5f,-.5f,-.5f),    RgbaFloat.Yellow),
            // Front
            new VertexPositionColor(new Vector3(-.5f,.5f,.5f),     RgbaFloat.Cyan),
            new VertexPositionColor(new Vector3(.5f,.5f,.5f),      RgbaFloat.Cyan),
            new VertexPositionColor(new Vector3(.5f,-.5f,.5f),     RgbaFloat.Cyan),
            new VertexPositionColor(new Vector3(-.5f,-.5f,.5f),    RgbaFloat.Cyan)
        };

        private static readonly int[] s_cubeIndices = new int[]
        {
            0,1,2, 0,2,3,
            4,5,6, 4,6,7,
            8,9,10, 8,10,11,
            12,13,14, 12,14,15,
            16,17,18, 16,18,19,
            20,21,22, 20,22,23,
        };

        private static readonly string VertexShaderSource = "simple-vertex";
        private static readonly string FragmentShaderSource = "simple-frag";
    }
}
