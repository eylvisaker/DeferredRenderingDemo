﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBufferDemoLib
{
    public class FillGBufferEffect: Effect
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
        private EffectParameter p_Emissive;
        private EffectParameter p_SpecularExponent;
        private EffectParameter p_SpecularIntensity;
        private EffectTechnique t_Sprite;
        private EffectTechnique t_Textured;
        private EffectTechnique t_BumpMapped;
        private Matrix world, viewProjection;

        public FillGBufferEffect(Effect effect) : base(effect)
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
            p_Emissive = effect.Parameters["Emissive"];
            p_SpecularExponent = effect.Parameters["SpecularExponent"];
            p_SpecularIntensity = effect.Parameters["SpecularIntensity"];

            t_Sprite = effect.Techniques["Sprite"];
            t_Textured = effect.Techniques["Textured"];
            t_BumpMapped = effect.Techniques["Bumped"];
        }

        /// <summary>
        /// Technique for vertex data which includes only position and texture coords.
        /// The default normal is applied.
        /// </summary>
        public EffectTechnique TechniqueSprite => t_Sprite;
        public EffectTechnique TechniqueTextured => t_Textured;
        public EffectTechnique TechniqueBumpMapped => t_BumpMapped;

        /// <summary>
        /// Emissive is a float from 0-1 that is written with every visible pixel recorded.
        /// A pixel with emissive value 1 will be fully lit regardless of ambient or directional
        /// light settings..
        /// </summary>
        public float Emissive
        {
            get => p_Emissive.GetValueSingle();
            set => p_Emissive.SetValue(value);
        }

        public int ApplyDesat
        {
            get => p_ApplyDesat.GetValueInt32();
            set => p_ApplyDesat.SetValue(value);
        }

        public float SpecularExponent
        {
            get => p_SpecularExponent.GetValueSingle();
            set => p_SpecularExponent.SetValue(value);
        }

        public float SpecularIntensity
        {
            get => p_SpecularIntensity.GetValueSingle();
            set => p_SpecularIntensity.SetValue(value);
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