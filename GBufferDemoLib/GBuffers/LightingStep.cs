using GBufferDemoLib.Cameras;
using GBufferDemoLib.GBuffers.Effects;
using GBufferDemoLib.Geometry;
using GBufferDemoLib.Lights;
using GBufferDemoLib.Shadows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib.GBuffers
{
    public sealed class LightingStep : IDisposable
    {
        private readonly GraphicsDevice graphics;
        private readonly GBufferTargets targets;
        private readonly FullScreenDraw fullScreen;
        private Camera camera;
        private Model sphere;
        private LightGBufferEffect effect;

        public LightGBufferEffect LightEffect
        {
            get => effect;
            set
            {
                effect = value;

                if (sphere != null)
                {
                    sphere.Meshes[0].MeshParts[0].Effect = effect;
                }
            }
        }

        private DirectionalLightShadowEffect directionalLightEffect;

        public Vector3 DirectionToLight { get; set; }
        public Color DirLightColor { get; set; }

        public Color AmbientDown { get; set; } = new Color(20, 20, 20);
        public Color AmbientRange { get; set; } = new Color(60, 60, 60);

        public Model Sphere
        {
            get => sphere;
            set
            {
                sphere = value;
                sphere.Meshes[0].MeshParts[0].Effect = LightEffect;
            }
        }

        public float Gamma { get; internal set; }

        public LightingStep(GraphicsDevice graphics,
                            ContentManager content,
                            GBufferTargets targets,
                            FullScreenDraw fullScreen)
        {
            this.graphics = graphics;
            this.targets = targets;
            this.fullScreen = fullScreen;

            Sphere = content.Load<Model>("sphere");
            LightEffect = new LightGBufferEffect(content.Load<Effect>("LightGBuffer"));
            directionalLightEffect = new DirectionalLightShadowEffect(content.Load<Effect>("DirectionalLightShadow"));
        }

        public void Dispose()
        {
        }

        public void Begin(Camera camera)
        {
            this.camera = camera;

            Matrix viewProjectionInv = Matrix.Invert(camera.ViewProjection);

            LightEffect.Parameters["EyePosition"].SetValue(camera.Position);
            LightEffect.Parameters["ViewProjectionInv"].SetValue(viewProjectionInv);
            LightEffect.Parameters["TexelOffset"].SetValue(new Vector2(0, 0));
            LightEffect.Gamma = Gamma;

            directionalLightEffect.Parameters["ViewProjectionInv"].SetValue(viewProjectionInv);
            directionalLightEffect.Parameters["TexelOffset"].SetValue(new Vector2(0, 0));
            directionalLightEffect.Gamma = Gamma;

            LightEffect.ColorTexture = targets.Color;
            LightEffect.DepthTexture = targets.Depth;
            LightEffect.NormalTexture = targets.Normal;
            LightEffect.SpecularTexture = targets.Specular;

            graphics.BlendState = BlendState.Additive;
            graphics.DepthStencilState = DepthStencilState.None;
        }

        public void AmbientAndEmissive(Color ambientDown, Color ambientUp, float intensity)
        {
            AmbientAndEmissive(ambientDown.ToVector3() * intensity, ambientUp.ToVector3() * intensity);
        }

        public void AmbientAndEmissive(Vector3 ambientDownColor, Vector3 ambientUpColor)
        {
            LightEffect.Parameters["AmbientDown"].SetValue(ambientDownColor);
            LightEffect.Parameters["AmbientUpRange"].SetValue(ambientUpColor - ambientDownColor);

            LightEffect.CurrentTechnique = LightEffect.Techniques["AmbientAndEmissiveLighting"];

            fullScreen.Draw(LightEffect);
        }

        public void DirectionalLight(LightDirectional light)
        {
            if (light.EnableShadows && light.ShadowMapper != null)
            {
                graphics.SamplerStates[4] = CascadedShadowMapper.ShadowMapSamplerState;
                graphics.SamplerStates[5] = CascadedShadowMapper.ShadowMapSamplerState;
                graphics.SamplerStates[6] = CascadedShadowMapper.ShadowMapSamplerState;
                graphics.SamplerStates[7] = CascadedShadowMapper.ShadowMapSamplerState;

                directionalLightEffect.VisualizeCascades = light.ShadowMapper.Settings.VisualizeCascades;
                directionalLightEffect.FilterAcrossCascades = light.ShadowMapper.Settings.FilterAcrossCascades;
                directionalLightEffect.FilterSize = light.ShadowMapper.Settings.FixedFilterSize;
                directionalLightEffect.Bias = light.ShadowMapper.Settings.Bias;
                directionalLightEffect.OffsetScale = light.ShadowMapper.Settings.OffsetScale;
                directionalLightEffect.ShadowMatrix = light.ShadowMapper.ShadowMatrix;

                directionalLightEffect.ViewProjection = camera.ViewProjection;
                directionalLightEffect.CameraPosWS = camera.Position;

                directionalLightEffect.ColorTexture = targets.Color;
                directionalLightEffect.DepthTexture = targets.Depth;
                directionalLightEffect.NormalTexture = targets.Normal;
                directionalLightEffect.SpecularTexture = targets.Specular;

                directionalLightEffect.Apply(light);

                fullScreen.Draw(directionalLightEffect);
            }
            else
            {
                LightEffect.Parameters["DirToLight"].SetValue(light.DirectionToLight);
                LightEffect.Parameters["DirLightColor"].SetValue(light.ColorIntensity);

                LightEffect.CurrentTechnique = LightEffect.Techniques["DirectionalLighting"];

                fullScreen.Draw(LightEffect);
            }
        }

        public void PointLights(IReadOnlyList<PointLight> lights)
        {
            // Easiest way to make sure there is no funny business when the camera is near the surface of the sphere
            // is to actually cull the front faces of the sphere instead of its backfaces.
            RasterizerState prevState = graphics.RasterizerState;
            graphics.RasterizerState = RasterizerState.CullClockwise;

            foreach (PointLight light in lights)
            {
                ApplyLight(light);
            }

            graphics.RasterizerState = prevState;
        }

        private void ApplyLight(PointLight light)
        {
            // Compensate for sphere tesselation here.
            float range = light.Range * 1.01f;

            LightEffect.CurrentTechnique = LightEffect.Techniques["PointLight"];

            LightEffect.Parameters["PointLightPos"].SetValue(light.Position);
            LightEffect.Parameters["PointLightRangeReciprocal"].SetValue(1 / light.Range);
            LightEffect.Parameters["PointLightColor"].SetValue(light.Color.ToVector3());
            LightEffect.Parameters["PointLightIntensity"].SetValue(light.Intensity);

            LightEffect.WorldViewProjection = Matrix.CreateScale(range) * Matrix.CreateTranslation(light.Position) * camera.ViewProjection;

            sphere.Meshes[0].Draw();
        }
    }
}
