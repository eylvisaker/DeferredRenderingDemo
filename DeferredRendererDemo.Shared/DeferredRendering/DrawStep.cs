using DeferredRendererDemo.Cameras;
using DeferredRendererDemo.DeferredRendering.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeferredRendererDemo.DeferredRendering
{
    public class DrawStep
    {
        /// <summary>
        /// If true, the draw method should only draw objects which cast shadows.
        /// </summary>
        public bool ShadowCastersOnly { get; internal set; }

        /// <summary>
        /// The camera used when drawing.
        /// </summary>
        public Camera Camera { get; internal set; }
        public IDrawEffect Effect { get; internal set; }
        public GraphicsDevice GraphicsDevice { get; internal set; }
    }
}
