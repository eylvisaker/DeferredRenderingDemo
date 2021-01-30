using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GBufferDemo
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionNormalTangentsTexture : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Tangent0;
        public Vector3 Tangent1;
        public Vector2 TextureCoordinate;

        public VertexPositionNormalTangentsTexture(Vector3 position, Vector3 normal, Vector3 tangent0, Vector3 tangent1, Vector2 textureCoordinate)
        {
            Position = position;
            Normal = normal;
            Tangent0 = tangent0;
            Tangent1 = tangent1;
            TextureCoordinate = textureCoordinate;
        }

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;


        static VertexPositionNormalTangentsTexture()
        {
            VertexElement[] elements = new VertexElement[] 
            { 
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), 
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), 
                new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0), 
                new VertexElement(36, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
                new VertexElement(48, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0) 
            };

            VertexDeclaration declaration = new VertexDeclaration(elements);
            VertexDeclaration = declaration;
        }
    }
}
