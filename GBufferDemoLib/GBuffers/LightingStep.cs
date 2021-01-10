using GBufferDemoLib.GBuffers.Effects;
using GBufferDemoLib.Geometry;
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
        private Matrix viewProjection;
        private Model sphere;
        private LightGBufferEffect effect;

        public LightGBufferEffect Effect
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

        public Vector3 DirectionToLight { get; set; }
        public Color DirLightColor { get; set; }

        public Color AmbientDown { get; set; } = new Color(20, 20, 20);
        public Color AmbientRange { get; set; } = new Color(60, 60, 60);

        public Vector3 EyePosition { get; set; }
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public Model Sphere
        {
            get => sphere;
            set
            {
                sphere = value;
                sphere.Meshes[0].MeshParts[0].Effect = Effect;
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
            Effect = new LightGBufferEffect(content.Load<Effect>("LightGBuffer"));
        }

        public void Dispose()
        {
        }

        public void Begin()
        {
            viewProjection = View * Projection;
            
            Matrix viewProjectionInv = Matrix.Invert(viewProjection);

            Effect.Parameters["EyePosition"].SetValue(EyePosition);
            Effect.Parameters["ViewProjectionInv"].SetValue(viewProjectionInv);
            Effect.Gamma = Gamma;

            Effect.ColorTexture = targets.Color;
            Effect.DepthTexture = targets.Depth;
            Effect.NormalTexture = targets.Normal;
            Effect.SpecularTexture = targets.Specular;

            graphics.BlendState = BlendState.Additive;
            graphics.DepthStencilState = DepthStencilState.None;
        }

        public void AmbientAndEmissive(Color ambientDown, Color ambientUp, float intensity)
        {
            AmbientAndEmissive(ambientDown.ToVector3() * intensity, ambientUp.ToVector3() * intensity);
        }

        public void AmbientAndEmissive(Vector3 ambientDownColor, Vector3 ambientUpColor)
        {
            Effect.Parameters["AmbientDown"].SetValue(ambientDownColor);
            Effect.Parameters["AmbientUpRange"].SetValue(ambientUpColor - ambientDownColor);

            Effect.CurrentTechnique = Effect.Techniques["AmbientAndEmissiveLighting"];

            fullScreen.Draw(Effect);
        }

        public void DirectionalLight(Vector3 directionToLight, Color color)
        {
            DirectionalLight(directionToLight, color.ToVector3());
        }

        public void DirectionalLight(Vector3 directionToLight, Vector3 color)
        {
            directionToLight.Normalize();

            Effect.Parameters["DirToLight"].SetValue(directionToLight);
            Effect.Parameters["DirLightColor"].SetValue(color);

            Effect.CurrentTechnique = Effect.Techniques["DirectionalLighting"];

            fullScreen.Draw(Effect);
        }

        public void ApplyLights(IReadOnlyList<PointLight> lights)
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

            Effect.CurrentTechnique = Effect.Techniques["PointLight"];

            Effect.Parameters["PointLightPos"].SetValue(light.Position);
            Effect.Parameters["PointLightRangeReciprocal"].SetValue(1 / light.Range);
            Effect.Parameters["PointLightColor"].SetValue(light.Color.ToVector3());
            Effect.Parameters["PointLightIntensity"].SetValue(light.Intensity);

            Effect.WorldViewProjection = Matrix.CreateScale(range) * Matrix.CreateTranslation(light.Position) * viewProjection;

            sphere.Meshes[0].Draw();
        }
    }
}
