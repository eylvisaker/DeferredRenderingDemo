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

        public GBufferProcessor(GraphicsDevice graphics, GBuffer gbuffer)
        {
            this.graphics = graphics;
            this.gbuffer = gbuffer;

            InitializeFullScreen();
        }

        private static VertexPositionTexture[] fullScreen;
        private static int[] fullScreenIndices;

        public Vector3 DirectionToLight { get; internal set; }
        public Color DirLightColor { get; internal set; }

        private void InitializeFullScreen()
        {
            if (fullScreen != null)
                return;

            fullScreen = new[] {
                new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(-1,  1, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3( 1,  1, 0), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3( 1, -1, 0), new Vector2(1, 1)),
            };
            fullScreenIndices = new[] { 0, 1, 3, 1, 2, 3 };
        }

        internal void Begin(ProcessGBufferEffect effect, Matrix view, Matrix projection)
        {
            effect.Parameters["PerspectiveValues"].SetValue(new Vector4(
                1 / projection.M11, 1 / projection.M22, projection.M43, -projection.M33));
            effect.Parameters["ViewInv"].SetValue(Matrix.Invert(view));

            effect.ColorTexture = gbuffer.Color;
            // effect.DepthTexture = gbuffer.Depth;
            effect.NormalTexture = gbuffer.Normal;

            DirectionToLight.Normalize();

            effect.Parameters["AmbientDown"].SetValue(new Color(20, 20, 20).ToVector4());
            effect.Parameters["AmbientUpRange"].SetValue(new Color(60, 60, 60).ToVector4());
            effect.Parameters["DirToLight"].SetValue(DirectionToLight);
            effect.Parameters["DirLightColor"].SetValue(DirLightColor.ToVector3());

            effect.CurrentTechnique = effect.Techniques["DirectionalLighting"];

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, fullScreen, 0, 4, fullScreenIndices, 0, 2);
            }
        }
    }
}
