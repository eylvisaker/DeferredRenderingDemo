using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DeferredRendererDemo.DeferredRendering.Effects
{
    public class FillGBufferEffect : Effect, IEffectMatrices, IDrawEffect
    {
        private Matrix world, projection, view;
        private EffectParameter p_WorldViewProjection;
        private EffectParameter p_World;
        private EffectParameter p_SpriteNormal;
        private EffectParameter p_Color;
        private EffectParameter p_ApplyDesat;
        private EffectParameter p_Desat;
        private EffectParameter p_Devalue;
        private EffectParameter p_PreserveColor;
        private EffectParameter p_PreserveColorAngle;
        private EffectParameter p_DiffuseTexture;
        private EffectParameter p_NormalMapTexture;
        private EffectParameter p_SpecularMapTexture;
        private EffectParameter p_Emissive;
        private EffectParameter p_SpecularExponent;
        private EffectParameter p_SpecularIntensity;
        private EffectTechnique t_Sprite;
        private EffectTechnique t_Flat;
        private EffectTechnique t_Textured;
        private EffectTechnique t_BumpMapped;
        private EffectTechnique t_BumpSpecularMapped;
        private EffectTechnique t_InstanceFlat;
        private EffectTechnique t_InstanceTextured;
        private EffectTechnique t_InstanceBumpMapped;
        private EffectTechnique t_InstanceBumpSpecularMapped;
        private bool instancing;
        private Texture2D diffuseTexture;
        private Texture2D normalMapTexture;
        private Texture2D specularMapTexture;

        public FillGBufferEffect(Effect effect) : base(effect)
        {
            p_WorldViewProjection = Parameters["WorldViewProjection"];
            p_World = Parameters["World"];
            p_SpriteNormal = Parameters["SpriteNormal"];
            p_Color = Parameters["Color"];
            p_ApplyDesat = Parameters["ApplyDesat"];
            p_Desat = Parameters["Desat"];
            p_Devalue = Parameters["Devalue"];
            p_PreserveColor = Parameters["PreserveColor"];
            p_PreserveColorAngle = Parameters["PreserveColorAngle"];
            p_DiffuseTexture = Parameters["DiffuseTexture"];
            p_NormalMapTexture = Parameters["NormalMapTexture"];
            p_SpecularMapTexture = Parameters["SpecularMapTexture"];
            p_Emissive = Parameters["Emissive"];
            p_SpecularExponent = Parameters["SpecularExponent"];
            p_SpecularIntensity = Parameters["SpecularIntensity"];

            t_Sprite = Techniques["Sprite"];
            t_Flat = Techniques["Flat"];
            t_Textured = Techniques["Textured"];
            t_BumpMapped = Techniques["Bumped"];
            t_BumpSpecularMapped = Techniques["BumpSpeculared"];
            t_InstanceFlat = Techniques["Flat"];
            t_InstanceTextured = Techniques["InstanceTextured"];
            t_InstanceBumpMapped = Techniques["InstanceBumped"];
            t_InstanceBumpSpecularMapped = Techniques["InstanceBumpSpeculared"];

            Color = new Vector3(1, 1, 1);
            SpecularIntensity = 1;
        }

        /// <summary>
        /// Technique for vertex data which includes only position and texture coords.
        /// The default normal is applied.
        /// </summary>
        public EffectTechnique TechniqueSprite => t_Sprite;
        public EffectTechnique TechniqueFlat => t_Flat;
        public EffectTechnique TechniqueTextured => t_Textured;
        public EffectTechnique TechniqueBumpMapped => t_BumpMapped;
        public EffectTechnique TechniqueBumpSpecularMapped => t_BumpSpecularMapped;
        public EffectTechnique TechniqueInstanceFlat => t_InstanceFlat;
        public EffectTechnique TechniqueInstanceTextured => t_InstanceTextured;
        public EffectTechnique TechniqueInstanceBumpMapped => t_InstanceBumpMapped;
        public EffectTechnique TechniqueInstanceBumpSpecularMapped => t_InstanceBumpSpecularMapped;


        public Vector3 Color
        {
            get
            {
                Vector4 result = p_Color.GetValueVector4();
                return new Vector3(result.X, result.Y, result.Z);
            }
            set
            {
                p_Color.SetValue(new Vector4(value.X, value.Y, value.Z, 1));
            }
        }

        /// <summary>
        /// Emissive is a float from 0-1 that is written with every visible pixel recorded.
        /// A pixel with emissive value 1 will be fully lit regardless of ambient or directional
        /// light settings.
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
                p_World.SetValue(world);
            }
        }

        public Texture2D DiffuseTexture
        {
            get => diffuseTexture;
            set
            {
                diffuseTexture = value;
                p_DiffuseTexture.SetValue(value);
                
                SetTechnique();
            }
        }

        public Texture2D NormalMapTexture
        {
            get => normalMapTexture;
            set
            {
                normalMapTexture = value;
                p_NormalMapTexture.SetValue(value);
             
                SetTechnique();
            }
        }

        public Texture2D SpecularMapTexture
        {
            get => specularMapTexture;
            set
            {
                specularMapTexture = value;
                p_SpecularMapTexture.SetValue(value);
             
                SetTechnique();
            }
        }

        public bool Instancing
        {
            get => instancing;
            set
            {
                if (instancing == value)
                    return;

                instancing = value;

                SetTechnique();
            }
        }

        public Effect AsEffect() => this;

        public void SetTextures(Texture2D diffuse, Texture2D normalMap = null, Texture2D specularMap = null)
        {
            diffuseTexture = diffuse;
            normalMapTexture = normalMap;
            specularMapTexture = specularMap;

            p_DiffuseTexture.SetValue(diffuse);
            p_NormalMapTexture.SetValue(normalMap);
            p_SpecularMapTexture.SetValue(specularMap);

            SetTechnique();
        }

        private void SetTechnique()
        {
            bool d = diffuseTexture != null;
            bool n = normalMapTexture != null;
            bool s = specularMapTexture != null;

            if (instancing)
            {
                if (d && n && s)
                    CurrentTechnique = TechniqueInstanceBumpSpecularMapped;
                else if (d && n)
                    CurrentTechnique = TechniqueInstanceBumpMapped;
                else if (d)
                    CurrentTechnique = TechniqueInstanceTextured;
                else
                    CurrentTechnique = TechniqueInstanceFlat;
            }
            else
            {
                if (d && n && s)
                    CurrentTechnique = TechniqueBumpSpecularMapped;
                else if (d && n)
                    CurrentTechnique = TechniqueBumpMapped;
                else if (d)
                    CurrentTechnique = TechniqueTextured;
                else
                    CurrentTechnique = TechniqueFlat;
            }
        }
    }
}