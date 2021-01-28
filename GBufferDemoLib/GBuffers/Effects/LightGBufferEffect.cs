using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBufferDemoLib.GBuffers.Effects
{
    public class LightGBufferEffect : Effect
    {
        private EffectParameter p_ColorTexture;
        private EffectParameter p_DepthTexture;
        private EffectParameter p_NormalTexture;
        private EffectParameter p_SpecularTexture;
        private EffectParameter p_Gamma;
        private EffectParameter p_WorldViewProjection;

        public LightGBufferEffect(Effect copyFrom) : base(copyFrom)
        {
            p_ColorTexture = Parameters["ColorTexture"];
            p_DepthTexture = Parameters["DepthTexture"];
            p_NormalTexture = Parameters["NormalTexture"];
            p_SpecularTexture = Parameters["SpecularTexture"];
            p_Gamma = Parameters["Gamma"];

            p_WorldViewProjection = Parameters["WorldViewProjection"];
        }

        public float Gamma
        {
            get => p_Gamma.GetValueSingle();
            set => p_Gamma.SetValue(value);
        }

        public Texture2D ColorTexture
        {
            get => p_ColorTexture.GetValueTexture2D();
            set => p_ColorTexture?.SetValue(value);
        }

        public Texture2D DepthTexture
        {
            get => p_DepthTexture.GetValueTexture2D();
            set => p_DepthTexture?.SetValue(value);
        }

        public Texture2D NormalTexture
        {
            get => p_NormalTexture.GetValueTexture2D();
            set => p_NormalTexture?.SetValue(value);
        }

        public Texture2D SpecularTexture
        {
            get => p_SpecularTexture.GetValueTexture2D();
            set => p_SpecularTexture?.SetValue(value);
        }

        public Matrix WorldViewProjection
        {
            get => p_WorldViewProjection.GetValueMatrix();
            set => p_WorldViewProjection.SetValue(value);
        }
    }
}