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

            gbufferProc.effect = fEffect;

            InitLights();
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

            rot += 0.05f * new Vector3(1, 0.82f, 0.71f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
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

                InitLights();
            }

            // Useful for debugging.
            // BasicEffectDraw(graphics);


            GBufferDraw(graphics);

            base.Draw(gameTime);
        }

        private void InitLights()
        {
            Random r = new Random();

            lights.Clear();
            foreach (Vector3 pt in LatticePoints(0))
            {
                lights.Add(new Light
                {
                    Color = new Color((150 + pt.X * 40) % 256, (250 + pt.Y * 50) % 256, (80 + pt.Z * 60) % 256),
                    Range = 20/* + (float)Math.Sin(rot.Z * 10) * 5*/,
                    Position = pt + 5f * new Vector3((float)Math.Cos(rot.X + pt.X), (float)Math.Sin(rot.X + pt.X), 0),
                    Intensity = 120/* + (float)Math.Cos(rot.X * 500 + pt.Length())*/,
                });
            }
        }

        private void GBufferDraw(GraphicsDevice graphics)
        {
            graphics.DepthStencilState = DepthStencilState.Default;
            graphics.BlendState = BlendState.Opaque;

            Matrix view = Matrix.CreateLookAt(new Vector3(-10, -10, -6),
                                                new Vector3(0, 0, 0),
                                                new Vector3(0, 0, 1));
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1000);

            InitLights();

            gEffect.ViewProjection = view * projection;

            gbuffer.Bind();

            RenderGeometry(graphics);

            gbuffer.Complete();

            gbufferProc.DirectionToLight = new Vector3((float)Math.Cos(rot.X * 100), (float)Math.Sin(rot.X * 100), 1);
            gbufferProc.DirLightColor = Color.Black;
            gbufferProc.AmbientDown = new Color(20, 30, 40);
            gbufferProc.AmbientRange = new Color(40, 30, 20); 

            gbufferProc.Begin(view, projection);

            foreach (Light light in lights)
            {
                gbufferProc.PointLight(light);
            }
        }

        private void RenderGeometry(GraphicsDevice graphics)
        {
            gEffect.ApplyDesat = 0;
            gEffect.DiffuseTexture = surface;

            var effect = gEffect.Effect;

            effect.CurrentTechnique = effect.Techniques["Textured"];

            graphics.SamplerStates[0] = SamplerState.AnisotropicClamp;

            foreach (Vector3 pt in LatticePoints(10))
            {
                gEffect.World = Matrix.CreateRotationX(rot.X * pt.X + pt.X) *
                                Matrix.CreateRotationX(rot.Y * pt.Y + pt.Y) *
                                Matrix.CreateRotationX(rot.Z * pt.Z + pt.Z) *
                                Matrix.CreateTranslation(pt); // *
                                //Matrix.CreateRotationX(rot.X) *
                                //Matrix.CreateRotationX(rot.Y) *
                                //Matrix.CreateRotationX(rot.Z);

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    graphics.Textures[0] = surface;
                    graphics.SetVertexBuffer(buffer);
                    graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, buffer.VertexCount / 3);
                }
            }
        }

        private IEnumerable<Vector3> LatticePoints(int distance = 10)
        {
            for (int k = 0; k <= distance; k++)
            {
                for (int j = 0; j <= distance; j++)
                {
                    for (int i = 0; i <= distance; i++)
                    {
                        yield return 10 * new Vector3(i, j, k);
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
