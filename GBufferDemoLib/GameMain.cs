using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GBufferDemoLib
{
    public class GameMain : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GBuffer gbuffer;
        private GBufferProcessor gbufferProc;
        private BasicEffect basicEffect;
        private VertexBuffer buffer;
        private FillGBufferEffect gEffect;
        private ProcessGBufferEffect fEffect;
        private Texture2D surface;
        private List<Light> lights = new List<Light>();

        public bool OpenGL { get; set; }

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
            this._graphics.ApplyChanges();

            basicEffect = new BasicEffect(GraphicsDevice);

            this.Window.AllowUserResizing = true;
            this.Window.AllowAltF4 = true;

            LoadContent();

            Window.ClientSizeChanged += Window_ClientSizeChanged;

            gbuffer = new GBuffer(GraphicsDevice, OpenGL);
            gbufferProc = new GBufferProcessor(GraphicsDevice, gbuffer);
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            gbuffer.Rebuild();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            buffer = new IcosahedronBuilder().CreateModel(GraphicsDevice);
            surface = Content.Load<Texture2D>("surface");

            gEffect = new FillGBufferEffect(Content.Load<Effect>("FillGBuffer"));
            fEffect = new ProcessGBufferEffect(Content.Load<Effect>("ProcessGBuffer"));
        }

        Vector3 rot;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);

            rot += 0.01f * new Vector3(1, 0.82f, 0.71f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }


        protected override void Draw(GameTime gameTime)
        {
            var graphics = GraphicsDevice;

            graphics.Clear(Color.DarkSlateGray);

            if (gEffect == null)
                return;

            bool rebuild = false;

            if (rebuild)
            {
                buffer.Dispose();
                buffer = new IcosahedronBuilder().CreateModel(GraphicsDevice);
            }

            // Useful for debugging.
            // BasicEffectDraw(graphics);


            GBufferDraw(graphics);

            base.Draw(gameTime);
        }

        private void GBufferDraw(GraphicsDevice graphics)
        {
            graphics.DepthStencilState = DepthStencilState.Default;

            Matrix view = Matrix.CreateLookAt(new Vector3(-5, -5, 0),
                                                new Vector3(0, 0, 0),
                                                new Vector3(0, 0, 1));
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1000);

            gEffect.ViewProjection = view * projection;

            gbuffer.Bind();

            RenderGeometry(graphics);

            gbuffer.Complete();

            gbufferProc.DirectionToLight = new Vector3((float)Math.Cos(rot.X * 100), (float)Math.Sin(rot.X * 100), 1);
            gbufferProc.DirLightColor = Color.White;

            gbufferProc.Begin(fEffect, view, projection);

        }

        private void RenderGeometry(GraphicsDevice graphics)
        {
            gEffect.ApplyDesat = 0;
            gEffect.DiffuseTexture = surface;

            var effect = gEffect.Effect;

            effect.CurrentTechnique = effect.Techniques["Textured"];

            graphics.SamplerStates[0] = SamplerState.AnisotropicClamp;

            for (int k = -10; k < 10; k++)
            {
                for (int j = -10; j < 10; j++)
                {
                    for (int i = -10; i < 10; i++)
                    {
                        gEffect.World =
                                        Matrix.CreateRotationX(rot.X * i + i) *
                                        Matrix.CreateRotationX(rot.Y * j + j) *
                                        Matrix.CreateRotationX(rot.Z * k + k) *
                                        Matrix.CreateTranslation(10 * new Vector3(i, j, k)) *
                                        Matrix.CreateRotationX(rot.X) *
                                        Matrix.CreateRotationX(rot.Y) *
                                        Matrix.CreateRotationX(rot.Z);

                        foreach (var pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            graphics.Textures[0] = surface;
                            graphics.SetVertexBuffer(buffer);
                            graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, buffer.VertexCount / 3);
                        }
                    }
                }
            }
        }

        private void BasicEffectDraw(GraphicsDevice graphics)
        {
            basicEffect.View = Matrix.CreateLookAt(new Vector3(-10, -10, 0),
                                                   new Vector3(0, 0, 0),
                                                   new Vector3(0, 0, 1));
            basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1000);
            basicEffect.TextureEnabled = true;
            basicEffect.LightingEnabled = false;

            basicEffect.Texture = surface;

            for (int k = -10; k < 10; k++)
            {
                for (int j = -10; j < 10; j++)
                {
                    for (int i = -10; i < 10; i++)
                    {
                        basicEffect.World = Matrix.CreateTranslation(50 * new Vector3(i, j, k)) *
                            Matrix.CreateRotationX(rot.X) *
                            Matrix.CreateRotationX(rot.Y) *
                            Matrix.CreateRotationX(rot.Z);

                        foreach (var pass in basicEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            graphics.Textures[0] = surface;
                            graphics.SetVertexBuffer(buffer);
                            graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, buffer.VertexCount / 3);
                        }
                    }
                }
            }
        }
    }
}
