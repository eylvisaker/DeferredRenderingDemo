using System;
using DeferredRendererDemo;
using DeferredRendererDemo.GBuffers;
using DeferredRendererDemo.Shadows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ShadowsSample.Components
{
    public class GameSettingsComponent : DrawableGameComponent
    {
        private static readonly int[] KernelSizes = { 2, 3, 5, 7 };
        private readonly GameMain game;

        private IGuiService _guiService;
        private KeyboardState _lastKeyboardState;
        private int bloomSettingsIndex;

        public bool AnimateLight { get => game.AnimateSun; set => game.AnimateSun = value; }
        public ShadowSettings ShadowSettings { get => game.Sky.Sun.Light.ShadowMapper.Settings; }
        public FixedFilterSize FixedFilterSize { get => ShadowSettings.FixedFilterSize; set => ShadowSettings.FixedFilterSize = value; }
        public bool VisualizeCascades { get => ShadowSettings.VisualizeCascades; set => ShadowSettings.VisualizeCascades = value; }
        public bool StabilizeCascades { get => ShadowSettings.StabilizeCascades; set => ShadowSettings.StabilizeCascades = value; }
        public bool FilterAcrossCascades { get => ShadowSettings.FilterAcrossCascades; set => ShadowSettings.FilterAcrossCascades = value; }

        public float Bias { get => ShadowSettings.Bias; set => ShadowSettings.Bias = value; }
        public float OffsetScale { get => ShadowSettings.OffsetScale; set => ShadowSettings.OffsetScale = value; }

        public int FixedFilterKernelSize
        {
            get { return KernelSizes[(int)FixedFilterSize]; }
        }

        public GameSettingsComponent(GameMain game)
            : base(game)
        {
            this.game = game;
        }

        public override void Initialize()
        {
            _guiService = Game.Services.GetService<IGuiService>();
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.L) && !_lastKeyboardState.IsKeyDown(Keys.L))
                AnimateLight = !AnimateLight;

            if (keyboardState.IsKeyDown(Keys.F) && !_lastKeyboardState.IsKeyDown(Keys.F))
            {
                FixedFilterSize++;
                if (FixedFilterSize > FixedFilterSize.Filter7x7)
                    FixedFilterSize = FixedFilterSize.Filter2x2;
            }

            if (keyboardState.IsKeyDown(Keys.C) && !_lastKeyboardState.IsKeyDown(Keys.C))
                StabilizeCascades = !StabilizeCascades;

            if (keyboardState.IsKeyDown(Keys.V) && !_lastKeyboardState.IsKeyDown(Keys.V))
                VisualizeCascades = !VisualizeCascades;

            if (keyboardState.IsKeyDown(Keys.K) && !_lastKeyboardState.IsKeyDown(Keys.K))
                FilterAcrossCascades = !FilterAcrossCascades;

            if (keyboardState.IsKeyDown(Keys.B) && !_lastKeyboardState.IsKeyDown(Keys.B))
            {
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    Bias += 0.001f;
                }
                else
                {
                    Bias -= 0.001f;
                    Bias = Math.Max(Bias, 0.0f);
                }
                Bias = (float)Math.Round(Bias, 3);
            }

            if (keyboardState.IsKeyDown(Keys.O) && !_lastKeyboardState.IsKeyDown(Keys.O))
            {
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    OffsetScale += 0.1f;
                }
                else
                {
                    OffsetScale -= 0.1f;
                    OffsetScale = Math.Max(OffsetScale, 0.0f);
                }
                OffsetScale = (float)Math.Round(OffsetScale, 1);
            }

            if (keyboardState.IsKeyDown(Keys.Z) && !_lastKeyboardState.IsKeyDown(Keys.Z))
            {
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
                {
                    bloomSettingsIndex--;
                    if (bloomSettingsIndex < 0)
                        bloomSettingsIndex += BloomSettings.PresetSettings.Length;
                }
                else
                {
                    bloomSettingsIndex++;
                    bloomSettingsIndex %= BloomSettings.PresetSettings.Length;
                }

                game.GBuffer.BloomSettings = BloomSettings.PresetSettings[bloomSettingsIndex];
            }

            _lastKeyboardState = keyboardState;
        }

        public override void Draw(GameTime gameTime)
        {
            _guiService.DrawLabels(new[]
            {
                new GuiComponent.GuiLabelData { Name = "Animate sun? (L)", Value = AnimateLight.ToString() },
                new GuiComponent.GuiLabelData { Name = "Reset sun position (Enter)", Value = game.SunPos.ToString("0.0000") },
                new GuiComponent.GuiLabelData { Name = "Filter size (F)", Value = FixedFilterSize.ToString() },
                new GuiComponent.GuiLabelData { Name = "Stabilize cascades? (C)", Value = StabilizeCascades.ToString() },
                new GuiComponent.GuiLabelData { Name = "Visualize cascades? (V)", Value = VisualizeCascades.ToString() },
                new GuiComponent.GuiLabelData { Name = "Filter across cascades? (K)", Value = FilterAcrossCascades.ToString() },
                new GuiComponent.GuiLabelData { Name = "Bias (b / B)", Value = Bias.ToString() },
                new GuiComponent.GuiLabelData { Name = "Normal offset (o / O)", Value = OffsetScale.ToString() },
                new GuiComponent.GuiLabelData { Name = "Bloom Config (z / Z)", Value = bloomSettingsIndex.ToString() }
            }, Color.FromNonPremultiplied(100, 0, 0, 150));

            _guiService.DrawLabels(new[]
            {
                new GuiComponent.GuiLabelData { Name = "Point Light Count", Value = game.Scene.Lights.Count.ToString() },
            }, Color.FromNonPremultiplied(0, 100, 0, 150));
        }
    }
}