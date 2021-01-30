using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemo.Geometry
{
    public class IcosahedronBuilder
    {
        public void BuildGeometry(IGeometryBuilder geometryBuilder)
        {
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
            v.Add(new Vector3(goldenRatio, 0, 1));
            v.Add(new Vector3(-goldenRatio, 0, 1));

            // plane with x = 0
            v.Add(new Vector3(0, -1, -goldenRatio));
            v.Add(new Vector3(0, 1, -goldenRatio));
            v.Add(new Vector3(0, 1, goldenRatio));
            v.Add(new Vector3(0, -1, goldenRatio));

            // Normalize them so the points sit on the unit sphere
            for (int i = 0; i < v.Count; i++)
            {
                v[i].Normalize();
            }

            int[] vertexIndices = new[] {
                 1,     6, 10,  2,
                13,     3,  2, 10,
                11,     7,  3, 10,
                19,    11, 10,  6,
                 9,     7, 10, 11,

                 3,     1, 11,  6,
                17,     1,  6,  5,
                 7,     5,  6,  2,

                15,     9,  5,  2,
                 5,     9,  2,  3,

                16,    11,  1,  0,
                 8,     1,  8,  0,
                10,     5,  8,  1,

                 2,     9,  4,  8,
                18,     3,  4,  9,
                 4,     3,  7,  4,

                 6,    11,  0,  7,
                12,     5,  9,  8,
                20,     4,  0,  8,
                14,     7,  0,  4,

                000000000};

            for (int i = 0; i < 20; i++)
            {
                int startIndex = 4 * i;

                geometryBuilder.AddTriangle(vertexIndices[startIndex] - 1,
                                            v[vertexIndices[startIndex + 1]],
                                            v[vertexIndices[startIndex + 2]],
                                            v[vertexIndices[startIndex + 3]]);
            }
        }
    }
}
