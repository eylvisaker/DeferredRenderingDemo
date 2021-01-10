using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib
{
    public class GBuffer
    {
        private GraphicsDevice graphics;
        private readonly bool openGL;
        private RenderTarget2D color, depth, normal;
        
public RenderTarget2D Color => color;
        public RenderTarget2D Depth => depth;
        public RenderTarget2D Normal => normal;

        public GBuffer(GraphicsDevice graphics, bool openGL)
        {
            this.graphics = graphics;
            this.openGL = openGL;

            Rebuild();
        }

        internal void Begin()
        {
            graphics.SetRenderTargets(color, depth, normal);
            graphics.BlendState = BlendState.Opaque;
        }

        internal void Rebuild()
        {
            int backBufferWidth = graphics.PresentationParameters.BackBufferWidth;
            int backBufferHeight = graphics.PresentationParameters.BackBufferHeight;

            SurfaceFormat normalFormat = openGL ? SurfaceFormat.Color : SurfaceFormat.Color;// : SurfaceFormat.Rgba1010102;

            color = new RenderTarget2D(graphics, backBufferWidth, backBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            depth = new RenderTarget2D(graphics, backBufferWidth, backBufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            normal = new RenderTarget2D(graphics, backBufferWidth, backBufferHeight, false, normalFormat, DepthFormat.Depth24Stencil8);
        }

        internal void Complete()
        {
            graphics.SetRenderTargets();
        }
    }
}
