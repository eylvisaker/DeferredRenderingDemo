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
        private Texture2D white;
        private List<Light> lights = new List<Light>();

        private bool rebuild = false;

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
            white = Content.Load<Texture2D>("white");

            gEffect = new FillGBufferEffect(Content.Load<Effect>("FillGBuffer"));
            fEffect = new ProcessGBufferEffect(Content.Load<Effect>("ProcessGBuffer"));
        }

        Vector3 rot;
        private Vector3 eyePosition;
        private Matrix view;
        private Matrix projection;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);

            rot += 0.05f * new Vector3(1, 0.82f, 0.71f) * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                rebuild = true;
            }
        }


        protected override void Draw(GameTime gameTime)
        {
            var graphics = GraphicsDevice;

            graphics.Clear(Color.DarkSlateGray);

            if (gEffect == null)
                return;

            if (rebuild)
            {
                buffer.Dispose();
                buffer = new IcosahedronBuilder().CreateModel(GraphicsDevice);

                InitLights();

                rebuild = false;
            }

            eyePosition = new Vector3(-25, -25, -25);

            view = Matrix.CreateLookAt(eyePosition,
                                       new Vector3(0, 0, 0),
                                       new Vector3(0, 0, 1));
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 400);

            InitLights();


            // Useful for debugging.
            //BasicEffectDraw(graphics);


            GBufferDraw(graphics);

            base.Draw(gameTime);
        }

        private void InitLights()
        {
            Random r = new Random();

            lights.Clear();

            foreach (Vector3 pt in LatticePoints(10))
            {
                Color clr = new Color((int)(150 + pt.X * 17) % 256, (int)(250 + pt.Y * 11) % 256, (int)(80 + pt.Z * 31) % 256, 255);
                float phi = 40 * rot.X + pt.X + 10 * (rot.Y + pt.Y);

                lights.Add(new Light
                {
                    Color = clr,
                    Range = 15 + (float)Math.Sin(rot.Z * 10 * pt.Z + pt.X) * 3,
                    Position = pt + 2f * new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), (float) Math.Cos(30 * rot.X + pt.Z)),
                    Intensity = 2/* + (float)Math.Cos(rot.X * 500 + pt.Length())*/,
                });

                //lights.Add(new Light
                //{
                //    Color = new Color((150 + pt.X * 40) % 256, (250 + pt.Y * 50) % 256, (80 + pt.Z * 60) % 256),
                //    Range = 5/* + (float)Math.Sin(rot.Z * 10) * 5*/,
                //    Position = pt + new Vector3(2, 2, 2),
                //    Intensity = 10/* + (float)Math.Cos(rot.X * 500 + pt.Length())*/,
                //}); ;
            }
        }

        private void GBufferDraw(GraphicsDevice graphics)
        {
            graphics.DepthStencilState = DepthStencilState.Default;
            graphics.BlendState = BlendState.Opaque;

            gbufferProc.EyePosition = eyePosition;

            gEffect.ViewProjection = view * projection;

            gbuffer.Bind();

            RenderGeometry(graphics);

            gbuffer.Complete();

            gbufferProc.DirectionToLight = new Vector3((float)Math.Cos(rot.X), (float)Math.Sin(rot.X), 1);
            gbufferProc.DirLightColor = new Color(100, 80, 60);
            gbufferProc.AmbientDown = new Color(20, 30, 40);
            gbufferProc.AmbientRange = new Color(80, 70, 60);

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

            foreach (Vector3 pt in LatticePoints(10))
            {
                gEffect.World = Matrix.Identity *
                                  Matrix.CreateRotationX(rot.X * pt.X + pt.X) *
                                  Matrix.CreateRotationY(rot.Y * pt.Y + pt.Y) *
                                  Matrix.CreateRotationZ(rot.Z * pt.Z + pt.Z) *
                                 Matrix.CreateTranslation(pt) *
                                //Matrix.CreateRotationX(rot.X) *
                                //Matrix.CreateRotationY(rot.Y) *
                                //Matrix.CreateRotationZ(rot.Z) *
                                Matrix.Identity;

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    graphics.Textures[0] = surface;
                    graphics.SetVertexBuffer(buffer);
                    graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, buffer.VertexCount / 3);
                }
            }

            foreach (Light light in lights)
            {
                gEffect.World = Matrix.CreateScale(0.1f) *
                                Matrix.CreateTranslation(light.Position); // *
                                                                          //Matrix.CreateRotationX(rot.X) *
                                                                          //Matrix.CreateRotationX(rot.Y) *
                                                                          //Matrix.CreateRotationX(rot.Z);

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    graphics.Textures[0] = white;
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
            graphics.BlendState = BlendState.Opaque;
            graphics.DepthStencilState = DepthStencilState.Default;

            graphics.Clear(Color.Blue);

            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.TextureEnabled = true;
            basicEffect.LightingEnabled = false;

            basicEffect.Texture = surface;

            foreach (Vector3 pt in LatticePoints(1))
            {
                basicEffect.World = Matrix.Identity *
                                 // Matrix.CreateRotationX(rot.X * pt.X + pt.X) *
                                 // Matrix.CreateRotationY(rot.Y * pt.Y + pt.Y) *
                                 // Matrix.CreateRotationZ(rot.Z * pt.Z + pt.Z) *
                                 Matrix.CreateTranslation(pt.X + 4 * (float)Math.Sin(rot.X * 30 + pt.X), 
                                                          pt.Y + 4 * (float)Math.Cos(rot.X * 30 + pt.Y), pt.Z) *
                                //Matrix.CreateRotationX(rot.X) *
                                //Matrix.CreateRotationY(rot.Y) *
                                //Matrix.CreateRotationZ(rot.Z) *
                                Matrix.Identity;

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
