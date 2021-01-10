using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib.GBuffers
{
    public class GBufferInitParams
    {
        public GBufferInitParams()
        {
            Reach();
        }

        public SurfaceFormat Color { get; set; }
        public SurfaceFormat Depth { get; set; }
        public SurfaceFormat Normal { get; set; }
        public SurfaceFormat Specular { get; set; }
        public SurfaceFormat ColorAccumulation { get; set; }

        public void Reach()
        {
            Color = SurfaceFormat.Color;
            Depth = SurfaceFormat.Single;
            Normal = SurfaceFormat.Color;
            Specular = SurfaceFormat.Color;
            ColorAccumulation = SurfaceFormat.HalfVector4;
        }
    }
}
