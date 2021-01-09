using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBufferDemoLib
{
    public class FillGBufferEffect
    {
        private Effect effect;
        private EffectParameter p_WorldViewProjection;
        private EffectParameter p_World;
        private EffectParameter p_SpriteNormal;
        private EffectParameter p_ApplyDesat;
        private EffectParameter p_Desat;
        private EffectParameter p_Devalue;
        private EffectParameter p_PreserveColor;
        private EffectParameter p_PreserveColorAngle;
        private EffectParameter p_DiffuseTexture;
        private EffectParameter p_NormalMapTexture;
        private Matrix world, viewProjection;

        public FillGBufferEffect(Effect effect)
        {
            this.effect = effect;

            p_WorldViewProjection = effect.Parameters["WorldViewProjection"];
            p_World = effect.Parameters["World"];
            p_SpriteNormal = effect.Parameters["SpriteNormal"];
            p_ApplyDesat = effect.Parameters["ApplyDesat"];
            p_Desat = effect.Parameters["Desat"];
            p_Devalue = effect.Parameters["Devalue"];
            p_PreserveColor = effect.Parameters["PreserveColor"];
            p_PreserveColorAngle = effect.Parameters["PreserveColorAngle"];
            p_DiffuseTexture = effect.Parameters["DiffuseTexture"];
            p_NormalMapTexture = effect.Parameters["NormalMapTexture"];
        }

        public int ApplyDesat
        {
            get => p_ApplyDesat.GetValueInt32();
            set => p_ApplyDesat.SetValue(value);
        }

        public Matrix ViewProjection
        {
            get => viewProjection;
            set 
            {
                viewProjection = value;
                p_WorldViewProjection.SetValue(world * viewProjection);
            }
        }

        public Matrix World
        {
            get => world;
            set
            {
                world = value; 
                p_WorldViewProjection.SetValue(world * viewProjection);
                p_World.SetValue(world);
            }
        }

        public Effect Effect => effect;

        public Texture2D DiffuseTexture
        {
            get => p_DiffuseTexture.GetValueTexture2D();
            set => p_DiffuseTexture.SetValue(value);
        }

        public Texture2D NormalMapTexture
        {
            get => p_NormalMapTexture.GetValueTexture2D();
            set => p_NormalMapTexture.SetValue(value);
        }
    }
}