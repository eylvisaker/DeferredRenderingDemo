using GBufferDemo.Shadows;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemo.Lights
{
    public class LightDirectional
    {
        private Vector3 directionToLight;

        public Vector3 DirectionToLight
        {
            get => directionToLight; 
            set => directionToLight = Vector3.Normalize(value);
        }

        public Color Color { get; set; } = Color.White;

        public float Intensity { get; set; }

        public Vector3 ColorIntensity => Color.ToVector3() * Intensity;

        public CascadedShadowMapper ShadowMapper { get; set; }

        public bool EnableShadows { get; set; }
    }
}
