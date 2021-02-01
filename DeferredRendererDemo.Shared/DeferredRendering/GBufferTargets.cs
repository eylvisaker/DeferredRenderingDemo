using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeferredRendererDemo.DeferredRendering
{
    public sealed class GBufferTargets
        : IDisposable
    {
        private readonly GraphicsDevice graphics;
        private GBufferInitParams p;

        private RenderTarget2D color, depth, normal, specular, colorAccum;

        public RenderTarget2D Color => color;
        public RenderTarget2D Depth => depth;
        public RenderTarget2D Normal => normal;
        public RenderTarget2D Specular => specular;

        public RenderTarget2D ColorAccum => colorAccum;

        public RenderTarget2D LumAccumulator { get; set; }
        public RenderTarget2D LumStorage { get; set; }

        public GBufferTargets(GraphicsDevice graphics, GBufferInitParams p)
        {
            this.graphics = graphics;
            this.p = p;

            Rebuild();
        }

        public void Dispose()
        {
            DestroySurfaces();
        }

        public void Rebuild(GBufferInitParams newP = default)
        {
            p = newP ?? p;

            DestroySurfaces();

            int backBufferWidth = graphics.PresentationParameters.BackBufferWidth;
            int backBufferHeight = graphics.PresentationParameters.BackBufferHeight;

            color = new RenderTarget2D(graphics, backBufferWidth, backBufferHeight, false, p.Color, DepthFormat.Depth24Stencil8);
            depth = new RenderTarget2D(graphics, backBufferWidth, backBufferHeight, false, p.Depth, DepthFormat.Depth24Stencil8);
            normal = new RenderTarget2D(graphics, backBufferWidth, backBufferHeight, false, p.Normal, DepthFormat.Depth24Stencil8);
            specular = new RenderTarget2D(graphics, backBufferWidth, backBufferHeight, false, p.Specular, DepthFormat.Depth24Stencil8);

            colorAccum = new RenderTarget2D(graphics, backBufferWidth, backBufferHeight, false, p.ColorAccumulation, DepthFormat.None);

            int width  = backBufferWidth;
            int height = backBufferHeight;
            int steps = 0;

            while (width > 4 || height > 4)
            {
                width /= 2;
                height /= 2;
                steps++;
            }

            width = backBufferWidth;
            height = backBufferHeight;

            for (int i = 0; i < steps; i++)
            {
                if (width > 1) width /= 2;
                if (height > 1) height /= 2;
            }

            LumAccumulator = new RenderTarget2D(graphics, width, height, false, SurfaceFormat.HalfVector4, DepthFormat.None);
            LumStorage = new RenderTarget2D(graphics, width, height, false, SurfaceFormat.HalfVector4, DepthFormat.None);
        }

        private void DestroySurfaces()
        {
            color?.Dispose();
            depth?.Dispose();
            normal?.Dispose();
            specular?.Dispose();

            colorAccum?.Dispose();

            LumAccumulator?.Dispose();
            LumStorage?.Dispose();
        }
    }
}
