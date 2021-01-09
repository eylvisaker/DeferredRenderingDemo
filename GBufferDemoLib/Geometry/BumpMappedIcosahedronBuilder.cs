using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib.Geometry
{
    public class BumpMappedIcosahedronBuilder : IcosahedronBuilder
    {
        private List<VertexPositionNormalTangentsTexture> vertices;

        public override VertexBuffer CreateModel(GraphicsDevice graphics)
        {
            vertices = new List<VertexPositionNormalTangentsTexture>();

            BuildGeometry();

            var result = new VertexBuffer(graphics, VertexPositionNormalTangentsTexture.VertexDeclaration, 60, BufferUsage.None);

            result.SetData(vertices.ToArray());

            return result;
        }

        protected override void AddTriangle(int triangleIndex, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ba = b - a;
            Vector3 ca = c - a;
            Vector3 normal = Vector3.Cross(ca, ba);
            Vector3 tangent0 = b - c;
            Vector3 tangent1 = Vector3.Cross(normal, tangent0);

            normal.Normalize();
            tangent0.Normalize();
            tangent1.Normalize();

            // Inverse width of the texture.
            const float invW = 1 / 40.0f;

            vertices.Add(new VertexPositionNormalTangentsTexture
            {
                Position = a,
                Normal = normal,
                Tangent0 = tangent0,
                Tangent1 = tangent1,
                TextureCoordinate = new Vector2((1 + 2 * triangleIndex) * invW, 0),
            });

            vertices.Add(new VertexPositionNormalTangentsTexture
            {
                Position = b,
                Normal = normal,
                Tangent0 = tangent0,
                Tangent1 = tangent1,
                TextureCoordinate = new Vector2(2 * (triangleIndex + 1) * invW, 1),
            });

            vertices.Add(new VertexPositionNormalTangentsTexture
            {
                Position = c,
                Normal = normal,
                Tangent0 = tangent0,
                Tangent1 = tangent1,
                TextureCoordinate = new Vector2(2 * triangleIndex * invW, 1),
            });
        }
    }
}
