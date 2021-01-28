using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib
{
    public static class Colors
    {
        /// <summary>
        /// Returns a Color object calculated from hue, saturation and value.
        /// See algorithm at http://en.wikipedia.org/wiki/HSL_and_HSV#From_HSV
        /// </summary>
        /// <param name="hue">The hue angle in degrees.</param>
        /// <param name="saturation">A value from 0 to 1 representing saturation.</param>
        /// <param name="value">A value from 0 to 1 representing the value.</param>
        /// <returns></returns>
        public static Color FromHsv(double hue, double saturation, double value)
        {
            while (hue < 0)
            {
                hue += 360;
            }

            if (hue >= 360)
            {
                hue = hue % 360;
            }

            double hp = hue / 60;
            double chroma = value * saturation;
            double x = chroma * (1 - Math.Abs(hp % 2 - 1));

            double r1 = 0, b1 = 0, g1 = 0;

            switch ((int)hp)
            {
                case 0: r1 = chroma; g1 = x; break;
                case 1: r1 = x; g1 = chroma; break;
                case 2: g1 = chroma; b1 = x; break;
                case 3: g1 = x; b1 = chroma; break;
                case 4: r1 = x; b1 = chroma; break;
                case 5: r1 = chroma; b1 = x; break;
            }

            double m = value - chroma;

            return new Color((int)(255 * (r1 + m)), (int)(255 * (g1 + m)), (int)(255 * (b1 + m)));
        }

    }
}
