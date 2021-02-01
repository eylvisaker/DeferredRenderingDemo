using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeferredRendererDemo.DeferredRendering
{
    public class Averager
    {
        private GraphicsDevice graphics;
        private readonly FullScreenDraw fullScreen;
        private Effect effect;
        private EffectParameter p_Source;
        private EffectParameter p_a_weight;
        private EffectParameter p_b_weight;
        private EffectParameter p_a_texture;
        private EffectParameter p_b_texture;

        public Averager(GraphicsDevice graphics, ContentManager content, FullScreenDraw fullScreen)
        {
            this.graphics = graphics;
            this.fullScreen = fullScreen;
            this.effect = content.Load<Effect>("Stats");
            
            p_Source = effect.Parameters["Source"];
            p_a_weight = effect.Parameters["A_Weight"];
            p_b_weight = effect.Parameters["B_Weight"];
            p_a_texture = effect.Parameters["A_Texture"];
            p_b_texture = effect.Parameters["B_Texture"];
        }

        public RenderTarget2D Accumulator { get; set; }
        public Texture2D A { get; set; }
        public Texture2D B { get; set; }

        public void WeightedAverage(float a_weight, float b_weight)
        {
            graphics.BlendState = BlendState.Opaque;

            graphics.SetRenderTarget(Accumulator);

            effect.CurrentTechnique = effect.Techniques["Average"];

            p_a_weight.SetValue(new Vector4(a_weight, a_weight, a_weight, a_weight));
            p_b_weight.SetValue(new Vector4(b_weight, b_weight, b_weight, b_weight));
            p_a_texture.SetValue(A);
            p_b_texture.SetValue(B);
            effect.Parameters["HalfTexel"].SetValue(new Vector2(0.25f, 0.25f));

            fullScreen.Draw(effect);
        }
    }
}
