using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemo.GBuffers
{
    public class FullScreenDraw
    {
        private readonly GraphicsDevice graphics;
        private VertexBuffer vertexBuffer;

        public FullScreenDraw(GraphicsDevice graphics)
        {
            this.graphics = graphics;

            var vertices = new[] {
                new VertexPosition { Position = new Vector3(-1, 1, 0) },
                new VertexPosition { Position = new Vector3(1, 1, 0) },
                new VertexPosition { Position = new Vector3(-1, -1, 0) },
                new VertexPosition { Position = new Vector3(1, -1, 0) },
            };

            vertexBuffer = new VertexBuffer(graphics, VertexPosition.VertexDeclaration, 6, BufferUsage.None);

            vertexBuffer.SetData(vertices);
        }

        public void Draw(Effect effect)
        {
            graphics.RasterizerState = RasterizerState.CullCounterClockwise;
            
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.SetVertexBuffer(vertexBuffer);
                graphics.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }
    }
}
