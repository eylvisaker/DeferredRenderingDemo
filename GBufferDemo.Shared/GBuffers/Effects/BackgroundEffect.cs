using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemo.GBuffers.Effects
{
    public class BackgroundEffect : Effect
    {
        private EffectParameter p_WorldViewProjection;
        private EffectParameter p_BackgroundTexture;
        private EffectParameter p_Color;
        private EffectParameter p_DepthTexture;
        private EffectParameter p_Gamma;
        private Matrix projection;
        private Matrix view;
        private Matrix world;

        public BackgroundEffect(Effect effect) : base(effect)
        {
            CurrentTechnique = Techniques[0];

            p_WorldViewProjection = Parameters["WorldViewProjection"];
            p_BackgroundTexture = Parameters["BackgroundTexture"];
            p_Color = Parameters["Color"];
            p_Gamma = Parameters["Gamma"];
         
            p_DepthTexture = Parameters["DepthTexture"];

            p_Color.SetValue(new Vector4(1, 1, 1, 1));
        }

        public float Gamma
        {
            get => p_Gamma.GetValueSingle();
            set => p_Gamma.SetValue(value);
        }

        public Vector4 Color
        {
            get => p_Color.GetValueVector4();
            set => p_Color.SetValue(value);
        }

        public Texture2D DepthTexture
        {
            get => p_DepthTexture.GetValueTexture2D();
            set => p_DepthTexture?.SetValue(value);
        }

        public Matrix Projection
        {
            get => projection;
            set
            {
                projection = value;
                p_WorldViewProjection.SetValue(world * view * projection);
            }
        }

        public Matrix View
        {
            get => view;
            set
            {
                view = value;
                p_WorldViewProjection.SetValue(world * view * projection);
            }
        }

        public Matrix World
        {
            get => world;
            set
            {
                world = value;
                p_WorldViewProjection.SetValue(world * view * Projection);
            }
        }

        public Texture2D BackgroundTexture
        {
            get => p_BackgroundTexture.GetValueTexture2D();
            set => p_BackgroundTexture?.SetValue(value);
        }


        public Matrix WorldViewProjection => p_WorldViewProjection.GetValueMatrix();

    }
}
