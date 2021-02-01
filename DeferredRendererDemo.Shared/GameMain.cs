using DeferredRendererDemo.Cameras;
using DeferredRendererDemo.DeferredRendering;
using DeferredRendererDemo.DeferredRendering.Effects;
using DeferredRendererDemo.Geometry;
using DeferredRendererDemo.Lights;
using DeferredRendererDemo.Scenes;
using DeferredRendererDemo.Scenes.Village;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShadowsSample.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DeferredRendererDemo
{
    public class GameMain : Game
    {
        private GraphicsDeviceManager _graphics;
        private DeferredRenderer renderer;
        private Sky sky;
        private List<IScene> scenes = new List<IScene>();
        private IScene scene;

        private float sunPos = MathHelper.PiOver4 / 2;

        public Sky Sky => sky;
        public bool AnimateSun { get; set; }
        public float SunPos => sunPos;
        public DeferredRenderer Renderer => renderer;

        public IScene Scene => scene;

        public GameMain()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.SynchronizeWithVerticalRetrace = false;
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            this._graphics.PreferredBackBufferWidth = 1920;
            this._graphics.PreferredBackBufferHeight = 1080;
            this._graphics.SynchronizeWithVerticalRetrace = false;
            this._graphics.ApplyChanges();

            this.Window.AllowUserResizing = true;
            this.Window.AllowAltF4 = true;

            renderer = new DeferredRenderer(GraphicsDevice,
                                           new ContentManager(Content.ServiceProvider, Content.RootDirectory + "/gbuffer"),
                                           new GBufferInitParams());

            scenes.Add(new IcoScene(GraphicsDevice, Content));
            scenes.Add(new VillageScene(GraphicsDevice, Content));
            scene = scenes[0];

            LoadContent();

            Window.ClientSizeChanged += (sender, e) => renderer.RebuildTargets();

            Components.Add(new FramesPerSecondComponent(this));
            Components.Add(new GameSettingsComponent(this));

            var guiService = new GuiComponent(this);
            Components.Add(guiService);
            Services.AddService<IGuiService>(guiService);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            sky = new Sky(GraphicsDevice, Content);

            sky.Effect = renderer.BackgroundEffect;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Kbrd.Update();

            base.Update(gameTime);

            scene.Update(gameTime);

            if (Kbrd.KeyReleased(Keys.Enter))
            {
                sunPos = -0.2f;
            }

            if (Kbrd.IsKeyDown(Keys.LeftShift))
            {
                if (Kbrd.KeyReleased(Keys.D1))
                {
                    scene = scenes[0];
                }
                else if (Kbrd.KeyReleased(Keys.D2))
                {
                    scene = scenes[1];
                }
            }

            if (AnimateSun)
            {
                sunPos += 0.04f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (renderer.FillEffect == null)
                return;

            GBufferDraw(GraphicsDevice, gameTime);

            base.Draw(gameTime);
        }

        private void GBufferDraw(GraphicsDevice graphics, GameTime time)
        {
            UpdateSky();

            renderer.Camera = scene.Camera;
            renderer.Gamma = 2.2f;

            renderer.Begin(time);
            renderer.Clear();

            renderer.DrawGeometry(scene.Draw);
            renderer.ShadowMap(sky.Sun.Light, scene.Draw);

            LightingStep lighting = renderer.BeginLighting();

            lighting.AmbientAndEmissive(sky.Sun.AmbientDown, sky.Sun.AmbientUp);
            lighting.DirectionalLight(sky.Sun.Light);
            lighting.PointLights(scene.Lights);

            DrawSky(graphics);

            renderer.End(doBloom: true);
        }

        private void UpdateSky()
        {
            float angle = sunPos;

            sky.NightSkyRotation = angle * 0.1f;
            sky.Sun.DirectionTo = new Vector3(
                (float)Math.Cos(angle),
                0.707f,
                (float)Math.Sin(angle));
        }

        private void DrawSky(GraphicsDevice graphics)
        {
            graphics.DepthStencilState = DepthStencilState.None;

            sky.Draw(scene.Player.Position, scene.Camera.FarZ);

            graphics.DepthStencilState = DepthStencilState.Default;
        }
    }
}
