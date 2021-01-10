using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GBufferDemoLib.GBuffers
{
    public class Bloom
    {
        private readonly GraphicsDevice graphics;
        private readonly ContentManager content;
        private readonly FullScreenDraw fullScreen;
        private readonly SpriteBatch spriteBatch;
        private readonly Effect gaussianBlurEffect;

        private RenderTarget2D scratch, scratch2;
        private BloomSettings settings;

        public Bloom(GraphicsDevice graphicsDevice, ContentManager content, FullScreenDraw fullScreen)
        {
            this.graphics = graphicsDevice;
            this.content = content;
            this.fullScreen = fullScreen;
            this.spriteBatch = new SpriteBatch(graphicsDevice);

            gaussianBlurEffect = content.Load<Effect>("GaussianBlur");
        }

        /// <summary>
        /// Gets or sets the bloom settings.
        /// </summary>

        public Texture2D Blur(Texture2D image, BloomSettings settings)
        {
            this.settings = settings;
            
            int renderTargetWidth  = image.Width;
            int renderTargetHeight = image.Height;

            if (scratch == null || scratch.Width != renderTargetWidth || scratch.Height != renderTargetHeight)
            {
                scratch?.Dispose();
                scratch2?.Dispose();

                scratch = new RenderTarget2D(graphics, renderTargetWidth, renderTargetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None);
                scratch2 = new RenderTarget2D(graphics, renderTargetWidth, renderTargetHeight, false, SurfaceFormat.HalfVector4, DepthFormat.None);
            }

            // Pass 1: draw from rendertarget 1 into rendertarget 2,
            // using a shader to apply a horizontal gaussian blur filter.
            SetBlurEffectParameters(1.0f / renderTargetWidth, 0);

            DrawFullscreenQuad(scratch, image, gaussianBlurEffect);

            // Pass 2: draw from rendertarget 2 back into rendertarget 1,
            // using a shader to apply a vertical gaussian blur filter.
            SetBlurEffectParameters(0, 1.0f / renderTargetHeight);

            DrawFullscreenQuad(scratch2, scratch, gaussianBlurEffect);

            return scratch2;

            //graphics.SetRenderTarget(renderTarget);

            //EffectParameterCollection parameters = bloomCombineEffect.Parameters;

            //parameters["BloomIntensity"].SetValue(settings.BloomIntensity);
            //parameters["BaseIntensity"].SetValue(settings.BaseIntensity);
            //parameters["BloomSaturation"].SetValue(settings.BloomSaturation);
            //parameters["BaseSaturation"].SetValue(settings.BaseSaturation);

            //parameters["Image"].SetValue(image);
            //parameters["Bloom"].SetValue(scratch2);
        }


        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }

        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        float ComputeGaussian(float n)
        {
            float theta = settings.BlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }



        /// <summary>
        /// Helper for drawing a texture into a rendertarget, using
        /// a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFullscreenQuad(RenderTarget2D renderTarget, Texture2D texture,
            Effect effect)
        {
            graphics.SetRenderTarget(renderTarget);

            effect.Parameters["ColorTexture"].SetValue(texture);

            fullScreen.Draw(effect);
        }
    }

    /// <summary>
    /// Class holds all the settings used to tweak the bloom effect.
    /// </summary>
    public class BloomSettings
    {
        #region Fields


        // Name of a preset bloom setting, for display to the user.
        public readonly string Name;


        // Controls how bright a pixel needs to be before it will bloom.
        // Zero makes everything bloom equally, while higher values select
        // only brighter colors. Somewhere between 0.25 and 0.5 is good.
        public readonly float BloomThreshold;


        // Controls how much blurring is applied to the bloom image.
        // The typical range is from 1 up to 10 or so.
        public readonly float BlurAmount;


        // Controls the amount of the bloom and base images that
        // will be mixed into the final scene. Range 0 to 1.
        public readonly float BloomIntensity;
        public readonly float BaseIntensity;


        // Independently control the color saturation of the bloom and
        // base images. Zero is totally desaturated, 1.0 leaves saturation
        // unchanged, while higher values increase the saturation level.
        public readonly float BloomSaturation;
        public readonly float BaseSaturation;


        #endregion


        /// <summary>
        /// Constructs a new bloom settings descriptor.
        /// </summary>
        public BloomSettings(string name, float bloomThreshold, float blurAmount,
                             float bloomIntensity, float baseIntensity,
                             float bloomSaturation, float baseSaturation)
        {
            Name = name;
            BloomThreshold = bloomThreshold;
            BlurAmount = blurAmount;
            BloomIntensity = bloomIntensity;
            BaseIntensity = baseIntensity;
            BloomSaturation = bloomSaturation;
            BaseSaturation = baseSaturation;
        }


        /// <summary>
        /// Table of preset bloom settings, used by the sample program.
        /// </summary>
        public static BloomSettings[] PresetSettings =
        {
            //                Name           Thresh  Blur Bloom  Base  BloomSat BaseSat
            new BloomSettings("Default",     0.25f,  4,   1.25f, 1,    1,       1),
            new BloomSettings("Soft",        0,      3,   1,     1,    1,       1),
            new BloomSettings("Desaturated", 0.5f,   8,   2,     1,    0,       1),
            new BloomSettings("Saturated",   0.25f,  4,   2,     1,    2,       0),
            new BloomSettings("Blurry",      0,      2,   1,     0.1f, 1,       1),
            new BloomSettings("Subtle",      0.5f,   2,   1,     1,    1,       1),
            new BloomSettings("Intense",     0.5f,   8,   2,     1,    2,       1),
        };
    }
}
