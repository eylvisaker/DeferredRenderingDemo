using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeferredRendererDemo.Geometry
{
    public interface IGeometryBuilder
    {
        SimpleGeometry CreateSimpleGeometry(GraphicsDevice graphics);

        void AddTriangle(int triangleIndex, Vector3 a, Vector3 b, Vector3 c);
    }

    public class BumpMappedGeometryBuilder : IGeometryBuilder
    {
        private List<VertexPositionNormalTangentsTexture> vertices = new List<VertexPositionNormalTangentsTexture>();
        private List<int> indices = new List<int>();

        public SimpleGeometry CreateSimpleGeometry(GraphicsDevice graphics)
        {
            SimpleGeometry result = new SimpleGeometry
            {
                Vertices = new VertexBuffer(graphics, VertexPositionNormalTangentsTexture.VertexDeclaration, vertices.Count, BufferUsage.None),
                Indices = new IndexBuffer(graphics, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.None)
            };

            result.Vertices.SetData(vertices.ToArray());
            result.Indices.SetData(indices.ToArray());

            return result;
        }

        public void AddTriangle(int triangleIndex, Vector3 a, Vector3 b, Vector3 c)
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
            float invW = 1 / 40.0f;

            indices.Add(vertices.Count);
            indices.Add(vertices.Count + 1);
            indices.Add(vertices.Count + 2);

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

    public class TexturedGeometryBuilder : IGeometryBuilder
    {
        private List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
        private List<int> indices = new List<int>();

        public SimpleGeometry CreateSimpleGeometry(GraphicsDevice graphics)
        {
            SimpleGeometry result = new SimpleGeometry
            {
                Vertices = new VertexBuffer(graphics, VertexPositionNormalTexture.VertexDeclaration, vertices.Count, BufferUsage.None),
                Indices = new IndexBuffer(graphics, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.None)
            };

            result.Vertices.SetData(vertices.ToArray());
            result.Indices.SetData(indices.ToArray());

            return result;
        }

        public void AddTriangle(int triangleIndex, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ba = b - a;
            Vector3 ca = c - a;
            Vector3 normal = Vector3.Cross(ca, ba);

            normal.Normalize();

            // Inverse width of the texture.
            const float invW = 1 / 40.0f;

            indices.Add(vertices.Count);
            indices.Add(vertices.Count + 1);
            indices.Add(vertices.Count + 2);

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
