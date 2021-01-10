using Microsoft.Xna.Framework.Graphics;

namespace GBufferDemoLib
{
    public class ProcessGBufferEffect : Effect
    {
        private EffectParameter p_ColorTexture;
        private EffectParameter p_DepthTexture;
        private EffectParameter p_NormalTexture;
        private EffectParameter p_SpecularTexture;

        public ProcessGBufferEffect(Effect copyFrom) : base(copyFrom)
        {
            p_ColorTexture = Parameters["ColorTexture"];
            p_DepthTexture = Parameters["DepthTexture"];
            p_NormalTexture = Parameters["NormalTexture"];
            p_SpecularTexture = Parameters["SpecularTexture"];
        }

        public Texture2D ColorTexture
        {
            get => p_ColorTexture.GetValueTexture2D();
            set => p_ColorTexture.SetValue(value);
        }

        public Texture2D DepthTexture
        {
            get => p_DepthTexture.GetValueTexture2D();
            set => p_DepthTexture.SetValue(value);
        }

        public Texture2D NormalTexture
        {
            get => p_NormalTexture.GetValueTexture2D();
            set => p_NormalTexture.SetValue(value);
        }

        public Texture2D SpecularTexture
        {
            get => p_SpecularTexture.GetValueTexture2D();
            set => p_SpecularTexture.SetValue(value);
        }

        //   effect.CurrentTechnique = fEffect.Techniques["DirectionalLighting"];
    }
}