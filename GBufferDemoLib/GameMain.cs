using GBufferDemoLib.Cameras;
using GBufferDemoLib.GBuffers;
using GBufferDemoLib.GBuffers.Effects;
using GBufferDemoLib.Geometry;
using GBufferDemoLib.Lights;
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
        private BasicEffect basicEffect;
        private SimpleGeometry icosahedron;
        private Model sphere;
        private Model ship;
        private Model shipTexture;
        private Texture2D surface;
        private Texture2D surfaceNormalMap;
        private Texture2D surfaceSpecularMap;
        private Texture2D white;
        private Sky sky;
        private List<PointLight> lights = new List<PointLight>();
        private Player player = new Player();

        private bool rebuild = false;
        private int technique = 2;
        private float updateStep = 0.05f;

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
            this._graphics.SynchronizeWithVerticalRetrace = false;
            this._graphics.ApplyChanges();

            basicEffect = new BasicEffect(GraphicsDevice);

            this.Window.AllowUserResizing = true;
            this.Window.AllowAltF4 = true;

            gbuffer = new GBuffer(GraphicsDevice,
                                  new ContentManager(Content.ServiceProvider, Content.RootDirectory + "/gbuffer"),
                                  new GBufferInitParams());

            LoadContent();

            Window.ClientSizeChanged += (sender, e) => gbuffer.RebuildTargets();

            InitLights();

            icosahedronInstances = new InstanceDisplay(GraphicsDevice);

            farPlane = 9000;

            camera = new PerspectiveCamera(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, farPlane);
        }

        protected override void LoadContent()
        {
            InitIcosahedron();

            surface = Content.Load<Texture2D>("surface");
            surfaceNormalMap = Content.Load<Texture2D>("surface-normalmap");
            surfaceSpecularMap = Content.Load<Texture2D>("surface-specularmap");
            white = Content.Load<Texture2D>("white");
            sky = new Sky(GraphicsDevice, Content);
            sphere = Content.Load<Model>("highdefsphere");
            ship = Content.Load<Model>("ship_light");

            sky.Effect = gbuffer.BackgroundEffect;
        }

        private void InitIcosahedron()
        {
            var icosahedronBuilder = new IcosahedronBuilder();
            var geometry = new BumpMappedGeometryBuilder();
            icosahedronBuilder.BuildGeometry(geometry);

            icosahedron = geometry.CreateSimpleGeometry(GraphicsDevice);
        }

        private Vector3 rot;
        private int farPlane;
        private PerspectiveCamera camera;
        private KeyboardState lastKeyboard;
        private int latticeSize = 5;
        private bool paused;
        private bool drawInstanced = true;
        private InstanceDisplay icosahedronInstances;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);

            player.Update(gameTime, GamePad.GetState(PlayerIndex.One));

            if (!paused)
            {
                rot += updateStep * new Vector3(1, 0.82f, 0.71f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                rot.Z += updateStep * 0.71f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

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
            if (KeyReleased(ref keyboard, Keys.D3))
            {
                technique = 2;
            }
            if (KeyReleased(ref keyboard, Keys.OemPlus))
            {
                latticeSize++;
                rebuild = true;
            }
            if (KeyReleased(ref keyboard, Keys.Pause))
            {
                paused = !paused;
            }
            if (KeyReleased(ref keyboard, Keys.PageUp))
            {
                updateStep *= 2;
            }
            if (KeyReleased(ref keyboard, Keys.PageDown))
            {
                updateStep /= 2;
            }
            if (KeyReleased(ref keyboard, Keys.OemMinus))
            {
                latticeSize = Math.Max(0, latticeSize - 1);
                rebuild = true;
            }
            if (KeyReleased(ref keyboard, Keys.Q))
            {
                drawInstanced = false;
            }
            if (KeyReleased(ref keyboard, Keys.W))
            {
                drawInstanced = true;
            }
            if (KeyReleased(ref keyboard, Keys.A))
            {
                sky.Sun.Light.EnableShadows = false;
            }
            if (KeyReleased(ref keyboard, Keys.S))
            {
                sky.Sun.Light.EnableShadows = true;
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

            if (gbuffer.FillEffect == null)
                return;

            if (rebuild)
            {
                icosahedron.Dispose();
                InitIcosahedron();

                InitLights();

                rebuild = false;
            }

            camera.SetLookAt(player.Position, player.Position + player.Facing, player.Up);

            InitLights();

            // Useful for debugging.
            //BasicEffectDraw(graphics);


            GBufferDraw(graphics, gameTime);

            base.Draw(gameTime);
        }

        private void InitLights()
        {
            Random r = new Random();

            lights.Clear();

            foreach (Vector3 pt in LatticePoints(Math.Min(latticeSize, 10)))
            {
                Color clr = new Color((int)(150 + pt.X * 17) % 256, (int)(250 + pt.Y * 11) % 256, (int)(80 + pt.Z * 31) % 256, 255);
                float phi = 40 * rot.X + pt.X + 10 * (rot.Y + pt.Y);
                float range = 4; // + (float)Math.Sin(rot.Z * 100 + pt.Z + pt.X) * 3;
                float intensity = 50;
                Vector3 position = pt + 2f * new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), (float)Math.Cos(30 * rot.X * Math.Sin(pt.Z) + MathHelper.PiOver2));

                if (pt == Vector3.Zero)
                {
                    range = 100;
                    phi *= 0.2f;

                    intensity = 300;
                    clr = Color.White;
                    position = 100f * new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), 0);
                }

                lights.Add(new PointLight
                {
                    Color = clr,
                    Range = range,
                    Position = position,
                    Intensity = intensity,
                });
            }
        }

        private void GBufferDraw(GraphicsDevice graphics, GameTime time)
        {
            UpdateSky();

            gbuffer.Camera = camera;
            gbuffer.Gamma = 2.2f;

            gbuffer.Begin(time);
            gbuffer.Clear();

            gbuffer.DrawGeometry(DrawScene);
            gbuffer.ShadowMap(sky.Sun.Light, DrawScene);

            LightingStep lighting = gbuffer.BeginLighting();

            lighting.AmbientAndEmissive(sky.Sun.AmbientDown, sky.Sun.AmbientUp);
            lighting.DirectionalLight(sky.Sun.Light);
            lighting.ApplyLights(lights);

            DrawSky(graphics);

            gbuffer.End(doBloom: false);
        }

        private void UpdateSky()
        {
            float angle = rot.X - 0.2f;

            sky.NightSkyRotation = angle * 0.1f;
            sky.Sun.DirectionTo = new Vector3(
                (float)Math.Cos(angle),
                1,
                (float)Math.Sin(angle));
        }

        private void DrawScene(DrawStep drawStep)
        {
            drawStep.Effect.SpecularIntensity = 0;
            drawStep.Effect.SpecularExponent = 0;

            DrawLattice(drawStep);

            if (!drawStep.ShadowCastersOnly)
            {
                DrawLights(drawStep);
            }

            DrawShip(drawStep);
        }

        private void DrawShip(DrawStep drawStep)
        {
            drawStep.Effect.SetTextures(surface);
            drawStep.Effect.Emissive = 0;
            drawStep.Effect.Color = Color.White;
            drawStep.Effect.Instancing = false;

            float phi = (-0.1f * rot.X) % MathHelper.TwoPi;
            float heading = phi + MathHelper.Pi;

            Vector3 position = 100 * new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), 0) + 10 * Vector3.UnitZ;

            drawStep.Effect.PrepModel(ship);

            Matrix world = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationZ(heading) * Matrix.CreateTranslation(position);

            ship.Draw(world,
                      drawStep.Camera.View,
                      drawStep.Camera.Projection);
        }

        private void DrawLights(DrawStep drawStep)
        {
            drawStep.Effect.SetTextures(white);
            drawStep.Effect.Instancing = false;
            drawStep.Effect.PrepModel(sphere);

            for (int i = 0; i < lights.Count; i++)
            {
                PointLight light = lights[i];

                drawStep.Effect.Color = light.Color;
                drawStep.Effect.World = Matrix.CreateScale(0.1f) *
                                Matrix.CreateTranslation(light.Position);

                drawStep.Effect.Emissive = light.Intensity;

                if (i == 0)
                {
                    drawStep.Effect.World = Matrix.CreateTranslation(light.Position);
                }

                foreach (var mesh in sphere.Meshes)
                {
                    mesh.Draw();
                }
            }
        }

        private void DrawLattice(DrawStep drawStep)
        {
            var graphics = drawStep.GraphicsDevice;

            drawStep.Effect.Color = Color.White;
            drawStep.Effect.SpecularExponent = 100;
            drawStep.Effect.SpecularIntensity = 0.9f;

            switch (technique)
            {
                case 2:
                    drawStep.Effect.SetTextures(surface, surfaceNormalMap, surfaceSpecularMap);
                    break;
                case 1:
                    drawStep.Effect.SetTextures(surface, surfaceNormalMap);
                    break;
                case 0:
                default:
                    drawStep.Effect.SetTextures(surface);
                    break;
            }

            graphics.SamplerStates[0] = SamplerState.AnisotropicClamp;
            graphics.SamplerStates[1] = SamplerState.AnisotropicClamp;
            graphics.SamplerStates[2] = SamplerState.AnisotropicClamp;

            icosahedronInstances.Instances.Clear();

            foreach (Vector3 pt in LatticePoints(latticeSize))
            {
                var world = Matrix.CreateRotationX(rot.X * pt.X + pt.X) *
                            Matrix.CreateRotationY(rot.Y * pt.Y + pt.Y) *
                            Matrix.CreateRotationZ(rot.Z * pt.Z + pt.Z) *
                            Matrix.CreateTranslation(pt);

                if (pt == Vector3.Zero)
                {
                    Vector3 disp = new Vector3(0, 0, -3);
                    float scale = 400;
                    float dispScale = 0.515f;

                    Vector3 axis = new Vector3(-1, 1, 0);
                    axis.Normalize();
                    var q = Quaternion.CreateFromAxisAngle(axis, MathHelper.PiOver2 * 1.4f + MathHelper.Pi);

                    world = Matrix.CreateFromQuaternion(q) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(disp * scale * dispScale);
                }

                icosahedronInstances.Instances.Add(world);

                if (!drawInstanced)
                {
                    drawStep.Effect.World = world;
                    drawStep.Effect.Instancing = false;

                    foreach (var pass in drawStep.Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        graphics.Textures[0] = surface;
                        graphics.SetVertexBuffer(icosahedron.Vertices);
                        graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, icosahedron.Vertices.VertexCount / 3);
                    }
                }
            }

            if (drawInstanced)
            {
                drawStep.Effect.World = Matrix.Identity;
                drawStep.Effect.Instancing = true;

                icosahedronInstances.Draw(drawStep.Effect.AsEffect(), icosahedron);
            }
        }

        private void DrawSky(GraphicsDevice graphics)
        {
            graphics.DepthStencilState = DepthStencilState.None;

            sky.Draw(player.Position, farPlane);

            graphics.DepthStencilState = DepthStencilState.Default;
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

            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;
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
                    graphics.SetVertexBuffer(icosahedron.Vertices);
                    graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, icosahedron.Vertices.VertexCount / 3);
                }
            }
        }
    }
}
