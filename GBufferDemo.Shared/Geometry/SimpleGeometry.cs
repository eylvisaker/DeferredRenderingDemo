using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemo.Geometry
{
    public class SimpleGeometry
    {
        public VertexBuffer Vertices { get; set; }
        public IndexBuffer Indices { get; set; }

        public void Dispose()
        {
            Vertices.Dispose();
            Indices.Dispose();
        }
    }
}
