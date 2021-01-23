using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib.Shadows
{
    public static class Vector4Utility
    {
        public static Vector4 Round(Vector4 value)
        {
            return new Vector4(
                (float)Math.Round(value.X),
                (float)Math.Round(value.Y),
                (float)Math.Round(value.Z),
                (float)Math.Round(value.W));
        }

        public static Vector3 ToVector3(this Vector4 value)
        {
            return new Vector3(value.X, value.Y, value.Z) / value.W;
        }
    }
}
