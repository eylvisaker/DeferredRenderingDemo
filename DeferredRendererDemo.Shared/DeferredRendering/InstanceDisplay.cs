using DeferredRendererDemo.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeferredRendererDemo.DeferredRendering
{
    public class InstanceDisplay
    {
        private static VertexDeclaration InstanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight,1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );

        private DynamicVertexBuffer instanceVertexBuffer;
        private Matrix[] instanceTransforms;
        private readonly GraphicsDevice graphics;

        public List<Matrix> Instances = new List<Matrix>();

        public InstanceDisplay(GraphicsDevice graphics)
        {
            this.graphics = graphics;
        }

        public void Draw(Effect effect, SimpleGeometry geometry)
        {
            InitTransforms(geometry.Vertices);

            graphics.Indices = geometry.Indices;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList,
                                                 0,
                                                 0,
                                                 geometry.Vertices.VertexCount / 3,
                                                 0,
                                                 instanceTransforms.Length);
            }
        }

        private void InitTransforms(VertexBuffer modelVertices)
        {
            Array.Resize(ref instanceTransforms, Instances.Count);

            for (int i = 0; i < Instances.Count; i++)
                instanceTransforms[i] = Instances[i];

            if ((instanceVertexBuffer == null) || instanceTransforms.Length > instanceVertexBuffer.VertexCount)
            {
                instanceVertexBuffer?.Dispose();

                instanceVertexBuffer = new DynamicVertexBuffer(graphics,
                                                               InstanceVertexDeclaration,
                                                               instanceTransforms.Length,
                                                               BufferUsage.WriteOnly);

            }

            instanceVertexBuffer.SetData(
                instanceTransforms,
                0,
                instanceTransforms.Length,
                SetDataOptions.Discard);

            graphics.SetVertexBuffers(
                new VertexBufferBinding(modelVertices, 0, 0),
                new VertexBufferBinding(instanceVertexBuffer, 0, 1)
                );
        }
    }
}
