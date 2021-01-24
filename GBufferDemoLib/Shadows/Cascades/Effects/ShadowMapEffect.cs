using GBufferDemoLib.GBuffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib.Shadows.Cascades.Effects
{
    public class ShadowMapEffect : Effect, IDrawEffect
    {
        private readonly EffectParameter p_WorldViewProjection;
        private readonly EffectTechnique t_ShadowMap;
        private readonly EffectTechnique t_ShadowMapInstance;
        private Matrix world, projection, view;
        private bool instancing;

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

        public Matrix WorldViewProjection { get; set; }
        public Vector3 Color { get; set; }
        public float SpecularExponent { get; set; }
        public float SpecularIntensity { get; set; }
        public bool Instancing
        {
            get => instancing;
            set
            {
                if (value == instancing)
                    return;

                instancing = value;

                CurrentTechnique = instancing ? t_ShadowMapInstance : t_ShadowMap;
            }
        }

        public float Emissive { get; set; }

        public ShadowMapEffect(Effect innerEffect) : base(innerEffect)
        {
            p_WorldViewProjection = this.Parameters["WorldViewProjection"];

            t_ShadowMap = this.Techniques["ShadowMap"];
            t_ShadowMapInstance = this.Techniques["ShadowMapInstance"];
        }

        public Effect AsEffect() => this;

        public void SetTextures(Texture2D diffuse, Texture2D normalMap = null, Texture2D specularMap = null)
        {
            // No need for texture lookup when rendering to shadow maps.
        }
    }
}
