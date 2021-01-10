using GBufferDemoLib.Geometry;
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
        private GBuffer gbuffer;
        private GBufferProcessor gbufferProc;
        private BasicEffect basicEffect;
        private VertexBuffer buffer;
        private FillGBufferEffect gEffect;
        private ProcessGBufferEffect fEffect;
        private Texture2D surface;
        private Texture2D surfaceNormalMap;
        private Texture2D white;
        private Skybox skybox;
        private List<Light> lights = new List<Light>();
        private Player player = new Player();

        private bool rebuild = false;
        private int technique;

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
            buffer = new TexturedIcosahedronBuilder().CreateModel(GraphicsDevice);
            surface = Content.Load<Texture2D>("surface");
            surfaceNormalMap = Content.Load<Texture2D>("surface-normalmap");
            white = Content.Load<Texture2D>("white");
            skybox = new Skybox(GraphicsDevice, Content);

            gEffect = new FillGBufferEffect(Content.Load<Effect>("FillGBuffer"));
            fEffect = new ProcessGBufferEffect(Content.Load<Effect>("ProcessGBuffer"));
        }

        Vector3 rot;
        private Vector3 eyePosition;
        private Matrix view;
        private Matrix projection;
        private KeyboardState lastKeyboard;
        private int latticeSize = 5;
        Vector3 DirectionToSun => new Vector3((float)Math.Cos(rot.X), (float)Math.Sin(rot.X), 1);

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);

            player.Update(gameTime, GamePad.GetState(PlayerIndex.One));

            rot += 0.05f * new Vector3(1, 0.82f, 0.71f) * (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState keyboard = Keyboard.GetState();

            if (KeyReleased(ref keyboard, Keys.Space))
            {
                rebuild = true;
            }
            if (KeyReleased(ref keyboard, Keys.D1))
            {
                technique = 0;
            }
            if (KeyReleased(ref keyboard, Keys.D2))
            {
                technique = 1;
            }
            if (KeyReleased(ref keyboard, Keys.OemPlus))
            {
                latticeSize++;
                rebuild = true;
            }
            if (KeyReleased(ref keyboard, Keys.OemMinus))
            {
                latticeSize = Math.Max(0, latticeSize - 1);
                rebuild = true;
            }

            lastKeyboard = keyboard;
        }

        private bool KeyReleased(ref KeyboardState keyboard, Keys key)
        {
            return (lastKeyboard.IsKeyDown(key) && !keyboard.IsKeyDown(key));
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
                buffer = new BumpMappedIcosahedronBuilder().CreateModel(GraphicsDevice);

                InitLights();

                rebuild = false;
            }

            eyePosition = player.Position;

            view = Matrix.CreateLookAt(eyePosition,
                                       player.Position + player.Facing,
                                       player.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 900);

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

            foreach (Vector3 pt in LatticePoints(latticeSize))
            {
                Color clr = new Color((int)(150 + pt.X * 17) % 256, (int)(250 + pt.Y * 11) % 256, (int)(80 + pt.Z * 31) % 256, 255);
                float phi = 40 * rot.X + pt.X + 10 * (rot.Y + pt.Y);

                lights.Add(new Light
                {
                    Color = clr,
                    Range = 9 + (float)Math.Sin(rot.Z * 10 * pt.Z + pt.X) * 3,
                    Position = pt + 2f * new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), (float)Math.Cos(30 * rot.X + pt.Z)),
                    Intensity = 4/* + (float)Math.Cos(rot.X * 500 + pt.Length())*/,
                });
            }
        }

        private void GBufferDraw(GraphicsDevice graphics)
        {
            graphics.DepthStencilState = DepthStencilState.Default;
            graphics.BlendState = BlendState.Opaque;

            gbufferProc.EyePosition = eyePosition;

            gEffect.ViewProjection = view * projection;

            gbuffer.Begin();

            DrawScene(graphics);

            gbuffer.Complete();

            gbufferProc.DirectionToLight = DirectionToSun;
            gbufferProc.DirLightColor = new Color(100, 80, 60);
            gbufferProc.AmbientDown = new Color(20, 30, 40);
            gbufferProc.AmbientRange = new Color(80, 70, 60);

            gbufferProc.Begin(view, projection);

            foreach (Light light in lights)
            {
                gbufferProc.PointLight(light);
            }
        }

        private void DrawScene(GraphicsDevice graphics)
        {
            DrawSky();

            // gEffect.ApplyDesat = 0;
            gEffect.DiffuseTexture = surface;
            gEffect.NormalMapTexture = surfaceNormalMap;

            var effect = gEffect.Effect;

            switch (technique)
            {
                case 1:
                    effect.CurrentTechnique = effect.Techniques["Bumped"];
                    break;
                case 0:
                default:
                    effect.CurrentTechnique = effect.Techniques["Textured"];
                    break;
            }


            foreach (Vector3 pt in LatticePoints(latticeSize))
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

                gEffect.SpecularExponent = 4;
                gEffect.SpecularIntensity = 0.9f;

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

        private void DrawSky()
        {
            skybox.Effect = gEffect;

            skybox.Draw(player.Position);
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
