using GBufferDemoLib.Cameras;
using GBufferDemoLib.GBuffers.Effects;
using GBufferDemoLib.Lights;
using GBufferDemoLib.Shadows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBufferDemoLib.GBuffers
{
    public class GBuffer : IDisposable
    {
        private readonly FillGBufferEffect fillEffect;
        private readonly BackgroundEffect backgroundEffect;
        private readonly Effect postEffect;
        private readonly Effect clearEffect;

        private readonly GBufferTargets targets;
        private readonly LightingStep lighting;
        private readonly Downscaler downscaler;
        private readonly Bloom bloom;
        private readonly Averager luminanceAverager;
        private readonly GraphicsDevice graphics;
        private readonly ContentManager content;
        private readonly FullScreenDraw fullScreen;

        private readonly DrawStep drawStep = new DrawStep();

        private float timeStep;

        public BackgroundEffect BackgroundEffect => backgroundEffect;
        public FillGBufferEffect FillEffect => fillEffect;

        private LightingStep Light => lighting;

        public Matrix View => Camera.View;
        public Matrix Projection => Camera.Projection;
        public Vector3 EyePosition => Camera.Position;

        public Camera Camera { get; set; }

        public float Gamma { get; set; } = 1f;

        public BloomSettings BloomSettings { get; set; } = BloomSettings.PresetSettings[6];

        public GBuffer(GraphicsDevice graphics, ContentManager content, GBufferInitParams p)
        {
            this.graphics = graphics;
            this.content = content;
            fullScreen = new FullScreenDraw(graphics);

            fillEffect = new FillGBufferEffect(content.Load<Effect>("FillGBuffer"));
            backgroundEffect = new BackgroundEffect(content.Load<Effect>("Background"));
            postEffect = content.Load<Effect>("PostProcess");
            clearEffect = content.Load<Effect>("Clear");

            targets = new GBufferTargets(graphics, p);

            lighting = new LightingStep(graphics, content, targets, fullScreen);
            downscaler = new Downscaler(graphics, content, targets, fullScreen);
            bloom = new Bloom(graphics, content, fullScreen);

            luminanceAverager = new Averager(graphics, content, fullScreen);

            drawStep.GraphicsDevice = graphics;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                targets.Dispose();
                lighting.Dispose();
            }
        }

        public void Begin(GameTime time)
        {
            this.timeStep = (float)time.ElapsedGameTime.TotalSeconds;

            graphics.SetRenderTargets(targets.Color, targets.Depth, targets.Normal, targets.Specular);
        }

        public void DrawGeometry(Action<DrawStep> drawGeometry)
        {
            graphics.BlendState = BlendState.Opaque;
            graphics.DepthStencilState = DepthStencilState.Default;

            FillEffect.View = View;
            FillEffect.Projection = Projection;

            drawStep.Camera = Camera;
            drawStep.Effect = FillEffect;
            drawStep.ShadowCastersOnly = false;

            drawGeometry(drawStep);
        }


        public void ShadowMap(LightDirectional sunLight, Action<DrawStep> drawGeometry)
        {
            if (sunLight.ShadowMapper == null)
            {
                sunLight.ShadowMapper = new CascadedShadowMapper(graphics, new ShadowSettings(), content);
            }

            drawStep.ShadowCastersOnly = true;

            sunLight.ShadowMapper.RenderShadowMap(graphics, sunLight, Camera, (c, effect) =>
            {
                drawStep.Camera = c;
                drawStep.Effect = effect;

                drawGeometry(drawStep);
            });
        }

        public LightingStep BeginLighting()
        {
            Light.Gamma = Gamma;

            Light.Begin(Camera);

            graphics.SetRenderTargets(targets.ColorAccum);
            graphics.Clear(Color.Black);

            BackgroundEffect.View = View;
            BackgroundEffect.Projection = Projection;
            BackgroundEffect.DepthTexture = targets.Depth;
            BackgroundEffect.Gamma = Gamma;

            return Light;
        }

        public void End(bool doBloom = false)
        {
            downscaler.Downscale();

            postEffect.Parameters["GammaReciprocal"].SetValue(1 / Gamma);
            postEffect.Parameters["ColorTexture"].SetValue(targets.ColorAccum);
            postEffect.Parameters["AverageLuminanceTexture"].SetValue(AverageLuminance());
            postEffect.Parameters["MiddleGrey"].SetValue(150000f);
            postEffect.Parameters["LumWhiteSqr"].SetValue(1000000f);

            if (doBloom)
            {
                var bloomImage = this.bloom.Blur(targets.ColorAccum, BloomSettings);
                
                postEffect.CurrentTechnique = postEffect.Techniques["FinalBloom"];

                postEffect.Parameters["BloomIntensity"].SetValue(BloomSettings.BloomIntensity);
                postEffect.Parameters["BaseIntensity"].SetValue(BloomSettings.BaseIntensity);
                postEffect.Parameters["BloomSaturation"].SetValue(BloomSettings.BloomSaturation);
                postEffect.Parameters["BaseSaturation"].SetValue(BloomSettings.BaseSaturation);
                postEffect.Parameters["BloomTexture"].SetValue(bloomImage);
            }
            else
            {
                postEffect.CurrentTechnique = postEffect.Techniques["Final"];
            }

            graphics.SetRenderTargets();
            graphics.Clear(Color.Black);

            fullScreen.Draw(postEffect);
        }

        private Texture2D AverageLuminance()
        {
            RenderTarget2D accumulator = targets.LumAccumulator;
            RenderTarget2D a = targets.LumStorage;

            luminanceAverager.Accumulator = accumulator;
            luminanceAverager.A = a;
            luminanceAverager.B = downscaler.Largest(a.Width, a.Height);

            float bweight = 0.5f * timeStep;
            float aweight = 1 - bweight;

            luminanceAverager.WeightedAverage(aweight, bweight);

            targets.LumStorage = accumulator;
            targets.LumAccumulator = a;

            return accumulator;
        }

        public void Clear()
        {
            graphics.Clear(Color.Black);
            graphics.DepthStencilState = DepthStencilState.None;

            clearEffect.CurrentTechnique = clearEffect.Techniques["Clear"];

            fullScreen.Draw(clearEffect);
        }

        /// <summary>
        /// Rebuilds the render targets. Call this when the backbuffer size changes.
        /// </summary>
        public void RebuildTargets()
        {
            targets.Rebuild();
            downscaler.Rebuild();
        }
    }
}
