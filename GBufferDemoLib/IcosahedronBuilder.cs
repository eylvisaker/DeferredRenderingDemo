using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib
{
    class IcosahedronBuilder
    {
        public VertexBuffer CreateModel(GraphicsDevice graphics)
        {
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            List<int> indices = new List<int>();

            // An icosahedron can be created from the vertices of three intersecting golden rectangles.
            // https://en.wikipedia.org/wiki/Golden_rectangle
            List<Vector3> v = new List<Vector3>();

            const float goldenRatio = 1.6180339887498948482045868343656f;

            // plane with z = 0
            v.Add(new Vector3(-1, -goldenRatio, 0));
            v.Add(new Vector3(1, -goldenRatio, 0));
            v.Add(new Vector3(1, goldenRatio, 0));
            v.Add(new Vector3(-1, goldenRatio, 0));

            // plane with y = 0
            v.Add(new Vector3(-goldenRatio, 0, -1));
            v.Add(new Vector3(goldenRatio, 0, -1));
            v.Add(new Vector3(goldenRatio, 0,  1));
            v.Add(new Vector3(-goldenRatio, 0, 1));

            // plane with x = 0
            v.Add(new Vector3(0, -1, -goldenRatio));
            v.Add(new Vector3(0, 1, -goldenRatio));
            v.Add(new Vector3(0, 1, goldenRatio));
            v.Add(new Vector3(0, -1, goldenRatio));

            int[] vertexIndices = new[] {
                 1,     6,  2, 10,
                13,     3, 10,  2,
                11,     7, 10,  3,
                19,    11,  6, 10,
                 9,     7, 11, 10,
                      
                 3,     1,  6, 11,
                17,     1,  5,  6,
                 7,     5,  2,  6,
                      
                15,     9,  2,  5,
                 5,     9,  3,  2,

                16,    11,  0,  1,
                 8,     1,  0,  8,
                10,     5,  1,  8,
                      
                 2,     9,  8,  4,
                18,     3,  9,  4,
                 4,     3,  4,  7,
                      
                 6,    11,  7,  0,
                12,     5,  8,  9,
                20,     4,  8,  0,
                14,     7,  4,  0,

                000000000};

            for (int i = 0; i < 20; i++)
            {
                int startIndex = 4 * i;

                BuildTriangle(vertices,
                              vertexIndices[startIndex] - 1,
                              v[vertexIndices[startIndex + 1]],
                              v[vertexIndices[startIndex + 3]],
                              v[vertexIndices[startIndex + 2]]);
            }


            var result = new VertexBuffer(graphics, VertexPositionNormalTexture.VertexDeclaration, 60, BufferUsage.None);

            result.SetData(vertices.ToArray());

            return result;
        }

        private void BuildTriangle(List<VertexPositionNormalTexture> vertices, int triangleIndex, Vector3 a, Vector3 b, Vector3 c)
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
