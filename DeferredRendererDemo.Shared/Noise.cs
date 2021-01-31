using System;
using System.Collections.Generic;
using System.Text;

namespace DeferredRendererDemo
{
    /// <summary>
    /// Most of the formulas are pulled shamelessly from here: https://www.youtube.com/watch?v=LWFzPP8ZbdU&t=2906s
    /// </summary>
    public static class Noise
    {
        public static uint Squirrel3(int index, uint seed)
        {
            const uint bit_noise1 = 0xb5297a4d;
            const uint bit_noise2 = 0x68e31da4;
            const uint bit_noise3 = 0x1b56c4e9;

            uint mangled = (uint)index;

            mangled *= bit_noise1;
            mangled += seed;
            mangled ^= mangled >> 8;
            mangled += bit_noise2;
            mangled ^= mangled << 8;
            mangled *= bit_noise3;
            mangled ^= mangled >> 8;

            return mangled;
        }

        public static float Noise2D(int posX, int posY, uint seed, Func<int, uint, uint> noiseFunc = null)
        {
            return Noise2D_UInt32(posX, posY, seed, noiseFunc) / (float)UInt32.MaxValue;
        }

        public static uint Noise2D_UInt32(int posX, int posY, uint seed, Func<int, uint, uint> noiseFunc = null)
        {
            const int prime_number = 198491317; // Large prime number with non-boring bits

            int index = posX + prime_number * posY;

            if (noiseFunc != null)
                return noiseFunc(index, seed);
            else
                return Squirrel3(index, seed);
        }

        public static float Noise3D(int posX, int posY, int posZ, uint seed, Func<int, uint, uint> noiseFunc = null)
        {
            return Noise3D_UInt32(posX, posY, posZ, seed, noiseFunc) / (float)UInt32.MaxValue;
        }

        public static uint Noise3D_UInt32(int posX, int posY, int posZ, uint seed, Func<int, uint, uint> noiseFunc = null)
        {
            const int prime_number1 = 198491317; // Large prime number with non-boring bits
            const int prime_number2 = 6542989; // Large prime number with non-boring bits. Should be different length than prime_number1.

            int index = posX + prime_number1 * posY + posZ * prime_number2;

            if (noiseFunc != null)
                return noiseFunc(index, seed);
            else
                return Squirrel3(index, seed);
        }
    }
}