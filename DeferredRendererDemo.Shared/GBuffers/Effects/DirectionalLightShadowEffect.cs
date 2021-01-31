using DeferredRendererDemo.Lights;
using DeferredRendererDemo.Shadows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeferredRendererDemo.GBuffers.Effects
{
    public class DirectionalLightShadowEffect : LightGBufferEffect
    {
        private readonly EffectParameter p_cameraPosWS;
        private readonly EffectParameter p_shadowMatrix;
        private readonly EffectParameter p_cascadeSplits;
        private readonly EffectParameter p_cascadeOffsets;
        private readonly EffectParameter p_cascadeScales;
        private readonly EffectParameter p_bias;
        private readonly EffectParameter p_offsetScale;
        private readonly EffectParameter p_lightDirection;
        private readonly EffectParameter p_lightColor;
        private readonly EffectParameter p_diffuseColor;
        private readonly EffectParameter p_world;
        private readonly EffectParameter p_viewProjection;
        private readonly EffectParameter p_shadowMapSize;
        private readonly EffectParameter p_shadowMaps;

        public bool VisualizeCascades { get; set; }
        public bool FilterAcrossCascades { get; set; }
        public FixedFilterSize FilterSize { get; set; }

        public Vector3 CameraPosWS { get; set; }
        public Matrix ShadowMatrix { get; set; }
        public float[] CascadeSplits { get; private set; }
        public Vector4[] CascadeOffsets { get; private set; }
        public float Bias { get; set; }
        public float OffsetScale { get; set; }
        public Vector3 LightDirection { get; set; }
        public Vector3 LightColor { get; set; }
        public Vector3 DiffuseColor { get; set; }
        public Matrix World { get; set; }
        public Matrix ViewProjection { get; set; }

        public DirectionalLightShadowEffect(Effect cloneSource) : base(cloneSource)
        {
            p_cameraPosWS    = Parameters["CameraPosWS"];
            p_shadowMatrix   = Parameters["ShadowMatrix"];
            p_cascadeSplits  = Parameters["CascadeSplits"];
            p_cascadeOffsets = Parameters["CascadeOffsets"];
            p_cascadeScales  = Parameters["CascadeScales"];
            p_bias           = Parameters["Bias"];
            p_offsetScale    = Parameters["OffsetScale"];
            p_lightDirection = Parameters["LightDirection"];
            p_lightColor     = Parameters["LightColor"];
            p_diffuseColor   = Parameters["DiffuseColor"];
            p_world          = Parameters["World"];
            p_viewProjection = Parameters["ViewProjection"];
            p_shadowMapSize  = Parameters["shadowMapSize"];

            p_shadowMaps     = Parameters[$"ShadowMaps"];
        }

        public void Apply(LightDirectional light)
        {
            string techniqueName = "Visualize" + VisualizeCascades + "Filter" + FilterAcrossCascades + "FilterSize" + FilterSize;

            CurrentTechnique = Techniques[techniqueName];

            p_cameraPosWS.SetValue(CameraPosWS);
            p_shadowMatrix.SetValue(ShadowMatrix);
            p_cascadeSplits.SetValue(new Vector4(light.ShadowMapper.CascadeSplits[0], 
                                                 light.ShadowMapper.CascadeSplits[1],
                                                 light.ShadowMapper.CascadeSplits[2],
                                                 light.ShadowMapper.CascadeSplits[3]));
            p_cascadeOffsets.SetValue(light.ShadowMapper.CascadeOffsets);
            p_cascadeScales.SetValue(light.ShadowMapper.CascadeScales);
            p_bias.SetValue(Bias);
            p_offsetScale.SetValue(OffsetScale);
            p_lightDirection.SetValue(light.DirectionToLight);
            p_lightColor.SetValue(light.ColorIntensity);
            p_world.SetValue(World);
            p_viewProjection.SetValue(ViewProjection);
            p_shadowMapSize.SetValue(new Vector2(light.ShadowMapper.ShadowMaps.Width,
                                                 light.ShadowMapper.ShadowMaps.Height));

            p_shadowMaps.SetValue(light.ShadowMapper.ShadowMaps);
        }
    }
}
