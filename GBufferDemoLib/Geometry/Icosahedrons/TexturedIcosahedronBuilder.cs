using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib.Geometry.Icosahedrons
{
    public class TexturedIcosahedronBuilder : IcosahedronBuilder
    {
        private List<VertexPositionNormalTexture> vertices;

        public override VertexBuffer CreateModel(GraphicsDevice graphics)
        {
            vertices = new List<VertexPositionNormalTexture>();

            BuildGeometry();

            var result = new VertexBuffer(graphics, VertexPositionNormalTexture.VertexDeclaration, 60, BufferUsage.None);

            result.SetData(vertices.ToArray());

            return result;
        }

        protected override void AddTriangle(int triangleIndex, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ba = b - a;
            Vector3 ca = c - a;
            Vector3 normal = Vector3.Cross(ca, ba);

            normal.Normalize();

            // Inverse width of the texture.
            const float invW = 1 / 40.0f;

            vertices.Add(new VertexPositionNormalTexture
            {
                Position = a,
                Normal = normal,
                TextureCoordinate = new Vector2((1 + 2 * triangleIndex) * invW, 0),
            });

            vertices.Add(new VertexPositionNormalTexture
            {
                Position = b,
                Normal = normal,
                TextureCoordinate = new Vector2(2 * (triangleIndex + 1) * invW, 1),
            });

            vertices.Add(new VertexPositionNormalTexture
            {
                Position = c,
                Normal = normal,
                TextureCoordinate = new Vector2(2 * triangleIndex * invW, 1),
            });
        }
    }
}
