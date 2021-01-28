using GBufferDemoLib.Cameras;
using GBufferDemoLib.GBuffers;
using GBufferDemoLib.GBuffers.Effects;
using GBufferDemoLib.Geometry;
using GBufferDemoLib.Lights;
using GBufferDemoLib.Scenes;
using GBufferDemoLib.Scenes.Village;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GBufferDemoLib
{
    public class GameMain : Game
    {
        private GraphicsDeviceManager _graphics;
        private GBuffer gbuffer;
        private Sky sky;
        private List<IScene> scenes = new List<IScene>();
        private IScene scene;

        private float sunPos = MathHelper.PiOver4;
        private bool moveSun;

        public GameMain()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            this._graphics.PreferredBackBufferWidth = 1920;
            this._graphics.PreferredBackBufferHeight = 1080;
            this._graphics.SynchronizeWithVerticalRetrace = false;
            this._graphics.ApplyChanges();

            this.Window.AllowUserResizing = true;
            this.Window.AllowAltF4 = true;

            gbuffer = new GBuffer(GraphicsDevice,
                                  new ContentManager(Content.ServiceProvider, Content.RootDirectory + "/gbuffer"),
                                  new GBufferInitParams());

            scenes.Add(new VillageScene(GraphicsDevice, Content));
            scenes.Add(new IcoScene(GraphicsDevice, Content));
            scene = scenes[0];

            LoadContent();

            Window.ClientSizeChanged += (sender, e) => gbuffer.RebuildTargets();
        }

        protected override void LoadContent()
        {
            sky = new Sky(GraphicsDevice, Content);

            sky.Effect = gbuffer.BackgroundEffect;
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
                sunPos = 0;
                moveSun = !moveSun;
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

            if (moveSun)
            {
                sunPos += 0.04f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (gbuffer.FillEffect == null)
                return;

            GBufferDraw(GraphicsDevice, gameTime);

            base.Draw(gameTime);
        }

        private void GBufferDraw(GraphicsDevice graphics, GameTime time)
        {
            UpdateSky();

            gbuffer.Camera = scene.Camera;
            gbuffer.Gamma = 2.2f;

            gbuffer.Begin(time);
            gbuffer.Clear();

            gbuffer.DrawGeometry(scene.Draw);
            gbuffer.ShadowMap(sky.Sun.Light, scene.Draw);

            LightingStep lighting = gbuffer.BeginLighting();

            lighting.AmbientAndEmissive(sky.Sun.AmbientDown, sky.Sun.AmbientUp);
            lighting.DirectionalLight(sky.Sun.Light);
            lighting.PointLights(scene.Lights);

            DrawSky(graphics);

            gbuffer.End(doBloom: true);
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
