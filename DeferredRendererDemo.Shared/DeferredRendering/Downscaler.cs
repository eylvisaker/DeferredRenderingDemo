using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeferredRendererDemo.DeferredRendering
{
    public class Downscaler
    {
        private GraphicsDevice graphics;
        private GBufferTargets targets;
        private FullScreenDraw fullScreen;
        private Effect downscale;
        private RenderTarget2D[] renderTargets;
        Size screenSize;
        Size[] sizes;

        public Downscaler(GraphicsDevice graphics, ContentManager content, GBufferTargets targets, FullScreenDraw fullScreen)
        {
            this.graphics = graphics;
            this.targets = targets;
            this.fullScreen = fullScreen;

            this.downscale = content.Load<Effect>("Downscale");

            Rebuild();
        }

        public IReadOnlyList<Size> Sizes
        {
            get
            {
                if (screenSize.Width == graphics.PresentationParameters.BackBufferWidth
                    && screenSize.Height == graphics.PresentationParameters.BackBufferHeight)
                {
                    return sizes;
                }

                int startWidth = graphics.PresentationParameters.BackBufferWidth;
                int startHeight = graphics.PresentationParameters.BackBufferHeight;

                int width = startWidth;
                int height = startHeight;

                int steps = 0;

                while (width > 1 || height > 1)
                {
                    width /= 2;
                    height /= 2;

                    if (steps == 0)
                    {
                        width = NearestPowerOfTwo(width);
                        height = NearestPowerOfTwo(height);
                    }

                    steps++;
                }

                sizes = new Size[steps];

                width = startWidth;
                height = startHeight;

                for (int i = 0; i < steps; i++)
                {
                    if (width > 1) width /= 2;
                    if (height > 1) height /= 2;

                    if (i == 0)
                    {
                        width = NearestPowerOfTwo(width);
                        height = NearestPowerOfTwo(height);
                    }

                    sizes[i] = new Size { Width = width, Height = height };
                }

                return sizes;
            }
        }

        private int NearestPowerOfTwo(int val)
        {
            int lastDist = int.MaxValue;
            int thisDist = int.MaxValue;
            int i = 1;

            while (thisDist <= lastDist)
            {
                i *= 2;

                lastDist = thisDist;
                thisDist = Math.Abs(i - val);
            }

            return i / 2;
        }

        public IReadOnlyList<RenderTarget2D> Steps => renderTargets;

        public RenderTarget2D Largest(int width, int height)
        {
            return renderTargets.FirstOrDefault(x => x.Width <= width && x.Height <= height) ?? renderTargets.Last();
        }

        public void Downscale()
        {
            Texture2D last = targets.ColorAccum;

            graphics.BlendState = BlendState.Opaque;

            for (int i = 0; i < renderTargets.Length; i++)
            {
                Vector2 texelOffset = 0.5f *
                    new Vector2(1 / last.Width - 1 / renderTargets[i].Width,
                                1 / last.Height - 1 / renderTargets[i].Height);

                graphics.SetRenderTarget(renderTargets[i]);

                downscale.CurrentTechnique = downscale.Techniques["Downscale"];
                downscale.Parameters["ColorTexture"].SetValue(last);
                downscale.Parameters["TexelOffset"].SetValue(texelOffset);

                fullScreen.Draw(downscale);

                last = renderTargets[i];
            }
        }

        public void Rebuild()
        {
            DestroySurfaces();

            IReadOnlyList<Size> sizes = Sizes;

            renderTargets = new RenderTarget2D[sizes.Count];

            for (int i = 0; i < sizes.Count; i++)
            {
                renderTargets[i] = new RenderTarget2D(graphics,
                                                      sizes[i].Width,
                                                      sizes[i].Height,
                                                      false,
                                                      SurfaceFormat.HalfVector4,
                                                      DepthFormat.None);
            }
        }

        private void DestroySurfaces()
        {
            if (renderTargets == null)
                return;

            for (int i = 0; i < renderTargets.Length; i++)
            {
                renderTargets[i].Dispose();
            }
        }
    }

    public struct Size
    {
        public int Width, Height;
    }
}
