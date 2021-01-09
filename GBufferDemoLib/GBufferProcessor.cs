using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib
{
    public class GBufferProcessor
    {
        private readonly GraphicsDevice graphics;
        private readonly GBuffer gbuffer;

        private VertexPositionTexture[] fullScreen;
        private int[] fullScreenIndices;
        private VertexBuffer icosahedron;
        private Matrix viewProjection;

        public ProcessGBufferEffect effect { get; set; }

        public Vector3 DirectionToLight { get; internal set; }
        public Color DirLightColor { get; internal set; }

        public Color AmbientDown { get; set; } = new Color(20, 20, 20);
        public Color AmbientRange { get; set; } = new Color(60, 60, 60);

        public GBufferProcessor(GraphicsDevice graphics, GBuffer gbuffer)
        {
            this.graphics = graphics;
            this.gbuffer = gbuffer;

            fullScreen = new VertexPositionTexture[4];
            fullScreenIndices = new[] { 0, 1, 3, 1, 2, 3 };

            icosahedron = new IcosahedronBuilder().CreateModel(graphics);
        }
        private void InitializeFullScreen()
        {
            fullScreen[0] = new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1));
            fullScreen[1] = new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0));
            fullScreen[2] = new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0));
            fullScreen[3] = new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1));
        }

        internal void Begin(Matrix view, Matrix projection)
        {
            this.viewProjection = view * projection;

            InitializeFullScreen();

            graphics.BlendState = BlendState.Opaque;

            effect.Parameters["PerspectiveValues"].SetValue(new Vector4(
                1 / projection.M11, 1 / projection.M22, projection.M43, -projection.M33));
            effect.Parameters["ViewInv"].SetValue(Matrix.Invert(view));

            effect.ColorTexture = gbuffer.Color;
            effect.DepthTexture = gbuffer.Depth;
            effect.NormalTexture = gbuffer.Normal;

            DirectionToLight.Normalize();

            effect.Parameters["AmbientDown"].SetValue(AmbientDown.ToVector3());
            effect.Parameters["AmbientUpRange"].SetValue(AmbientRange.ToVector3());
            effect.Parameters["DirToLight"].SetValue(DirectionToLight);
            effect.Parameters["DirLightColor"].SetValue(DirLightColor.ToVector3());

            effect.CurrentTechnique = effect.Techniques["DirectionalLighting"];

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, fullScreen, 0, 4, fullScreenIndices, 0, 2);
            }

            graphics.BlendState = BlendState.Additive;
            graphics.DepthStencilState = DepthStencilState.None;
        }

        internal void PointLight(Light light)
        {
            effect.Parameters["PointLightPos"].SetValue(light.Position);
            effect.Parameters["PointLightRangeReciprocal"].SetValue(1 / light.Range);
            effect.Parameters["PointLightColor"].SetValue(light.Color.ToVector3());
            effect.Parameters["PointLightIntensity"].SetValue(light.Intensity);

            InitializeFullScreen();

            var screenPos = Vector4.Transform(new Vector4(light.Position, 1), viewProjection);
            var screenRange = Vector4.Transform(new Vector4(new Vector3(light.Range), 0), viewProjection);

            effect.CurrentTechnique = effect.Techniques["PointLight"];
            effect.Parameters["WorldViewProjection"].SetValue(Matrix.CreateScale(light.Range) * Matrix.CreateTranslation(light.Position) * viewProjection);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.SetVertexBuffer(icosahedron);
                graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, icosahedron.VertexCount / 3);
            }
        }
    }
}
