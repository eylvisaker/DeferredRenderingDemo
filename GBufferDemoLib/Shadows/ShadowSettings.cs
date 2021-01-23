using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib.Shadows
{
    public class ShadowSettings
    {
        public FixedFilterSize FixedFilterSize = FixedFilterSize.Filter2x2;
        public bool VisualizeCascades = true;
        public bool StabilizeCascades = true;
        public bool FilterAcrossCascades;
        public float SplitDistance0;
        public float SplitDistance1;
        public float SplitDistance2;
        public float SplitDistance3;
        public float Bias;
        public float OffsetScale;

        public int FixedFilterKernelSize
        {
            get { return (int)FixedFilterSize; }
        }

        public ShadowSettings()
        {
            Bias = 0.002f;
            OffsetScale = 0.0f;

            SplitDistance0 = 0.05f;
            SplitDistance1 = 0.15f;
            SplitDistance2 = 0.50f;
            SplitDistance3 = 1.0f;
        }
    }

    public enum FixedFilterSize
    {
        Filter2x2 = 2,
        Filter3x3 = 3,
        Filter5x5 = 5,
        Filter7x7 = 7
    }
}
