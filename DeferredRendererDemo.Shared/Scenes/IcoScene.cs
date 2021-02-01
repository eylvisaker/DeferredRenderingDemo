using DeferredRendererDemo.Cameras;
using DeferredRendererDemo.DeferredRendering;
using DeferredRendererDemo.DeferredRendering.Effects;
using DeferredRendererDemo.Geometry;
using DeferredRendererDemo.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeferredRendererDemo.Scenes
{
    public class IcoScene : IScene
    {
        private readonly GraphicsDevice graphics;
        private SimpleGeometry icosahedron;
        private Model ship;
        private Model sphere;

        private Texture2D surface;
        private Texture2D surfaceNormalMap;
        private Texture2D surfaceSpecularMap;
        private Texture2D white;

        private Vector3 rot;
        private PerspectiveCamera camera;
        private int latticeSize = 5;
        private bool paused;
        private bool drawInstanced = true;
        private InstanceDisplay icosahedronInstances;
        private int farPlane;
        private List<PointLight> lights = new List<PointLight>();
        private Player player = new Player();
        private BasicEffect basicEffect;

        private bool rebuild = false;
        private int technique = 2;
        private float updateStep = 0.05f;

        public Player Player => player;
        public Camera Camera => camera;

        public IReadOnlyList<PointLight> Lights => lights;

        public IcoScene(GraphicsDevice graphics, ContentManager content)
        {
            this.graphics = graphics;

            InitIcosahedron();
            LoadContent(content);

            InitLights();

            icosahedronInstances = new InstanceDisplay(graphics);

            basicEffect = new BasicEffect(graphics);


            farPlane = 900;

            camera = new PerspectiveCamera(MathHelper.PiOver4, graphics.Viewport.AspectRatio, 1, farPlane);

            float sqrt2 = (float)Math.Sqrt(2);

            player.Position = new Vector3(-60, -60, 6);
            player.Facing = Vector3.Normalize(new Vector3(2, 2, 0.15f));
        }

        private void LoadContent(ContentManager content)
        {
            ship = content.Load<Model>("ship_light");
            sphere = content.Load<Model>("highdefsphere");

            surface = content.Load<Texture2D>("surface");
            surfaceNormalMap = content.Load<Texture2D>("surface-normalmap");
            surfaceSpecularMap = content.Load<Texture2D>("surface-specularmap");
            white = content.Load<Texture2D>("white");
        }

        private void InitIcosahedron()
        {
            var icosahedronBuilder = new IcosahedronBuilder();
            var geometry = new BumpMappedGeometryBuilder();
            icosahedronBuilder.BuildGeometry(geometry);

            icosahedron = geometry.CreateSimpleGeometry(graphics);
        }

        internal void Rebuild()
        {
            icosahedron.Dispose();

            InitIcosahedron();
            InitLights();
        }

        private void InitLights()
        {
            const int randomSeed = 239847;
            const int noiseSeed = 293576234;

            Random r = new Random(randomSeed);

            lights.Clear();

            float phi = 2 * rot.X;

            lights.Add(new PointLight
            {
                Color = Color.White,
                Range = 100,
                Position = 100f * new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), 0),
                Intensity = 0.3f,
            });

            foreach (Vector3 pt in LatticePoints(latticeSize))
            {
                if (pt.Z > 50)
                {
                    if (Noise.Noise3D((int)pt.X, (int)pt.Y, (int)pt.Z, noiseSeed - 1) > Math.Sqrt(10 / (pt.Z - 49)))
                        continue;
                }
                if (pt.X > 40)
                {
                    if (Noise.Noise3D((int)pt.X, (int)pt.Y, (int)pt.Z, noiseSeed) > 0.1f + Math.Sqrt(10 / (pt.X - 39)))
                        continue;
                }
                if (pt.Y > 40)
                {
                    if (Noise.Noise3D((int)pt.X, (int)pt.Y, (int)pt.Z, noiseSeed + 1) > 0.1f + Math.Sqrt(10 / (pt.Y - 39)))
                        continue;
                }

                Color clr = Colors.FromHsv(r.Next(360), 1, 1);
                float range = 4; // + (float)Math.Sin(rot.Z * 100 + pt.Z + pt.X) * 3;
                float intensity = 0.05f;

                phi = 40 * rot.X + pt.X + 10 * (rot.Y + pt.Y);
                Vector3 position = pt + 2f * new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), (float)Math.Cos(30 * rot.X * Math.Sin(pt.Z) + MathHelper.PiOver2));

                lights.Add(new PointLight
                {
                    Color = clr,
                    Range = range,
                    Position = position,
                    Intensity = intensity,
                });
            }
        }

        public void Draw(DrawStep drawStep)
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

        private void DrawLattice(DrawStep drawStep)
        {
            var graphics = drawStep.GraphicsDevice;

            drawStep.Effect.Color = Color.White.ToVector3();
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
                Vector3 rotation = new Vector3(
                    (float)Math.Cos(this.rot.X * pt.X + pt.X),
                    (float)Math.Cos(this.rot.Y * pt.Y + pt.Y),
                    (float)Math.Cos(this.rot.Z * pt.Z + pt.Z));

                var world = Matrix.CreateRotationX(rotation.X) *
                            Matrix.CreateRotationY(rotation.Y) *
                            Matrix.CreateRotationZ(rotation.Z) *
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

        public void Update(GameTime gameTime)
        {
            player.Update(gameTime, GamePad.GetState(PlayerIndex.One));

            if (!paused)
            {
                rot += updateStep * new Vector3(1, 0.82f, 0.71f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                rot.Z += updateStep * 0.71f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (KeyReleased(Keys.Space))
            {
                rebuild = true;
            }
            if (KeyReleased(Keys.D1))
            {
                technique = 0;
            }
            if (KeyReleased(Keys.D2))
            {
                technique = 1;
            }
            if (KeyReleased(Keys.D3))
            {
                technique = 2;
            }
            if (KeyReleased(Keys.OemPlus))
            {
                latticeSize++;
                rebuild = true;
            }
            if (KeyReleased(Keys.Pause))
            {
                paused = !paused;
            }
            if (KeyReleased(Keys.PageUp))
            {
                updateStep *= 2;
            }
            if (KeyReleased(Keys.PageDown))
            {
                updateStep /= 2;
            }
            if (KeyReleased(Keys.OemMinus))
            {
                latticeSize = Math.Max(0, latticeSize - 1);
                rebuild = true;
            }
            if (KeyReleased(Keys.Q))
            {
                drawInstanced = false;
            }
            if (KeyReleased(Keys.W))
            {
                drawInstanced = true;
            }

            camera.SetLookAt(player.Position, player.Position + player.Facing, player.Up);

            InitLights();
        }

        private void DrawLights(DrawStep drawStep)
        {
            drawStep.Effect.SetTextures(white);
            drawStep.Effect.Instancing = false;
            drawStep.Effect.PrepModel(sphere);

            for (int i = 0; i < lights.Count; i++)
            {
                PointLight light = lights[i];

                drawStep.Effect.Color = light.Color.ToVector3() * 2f;
                drawStep.Effect.World = Matrix.CreateScale(0.1f) *
                                        Matrix.CreateTranslation(light.Position);

                drawStep.Effect.Emissive = 1f;

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


        private void DrawShip(DrawStep drawStep)
        {
            drawStep.Effect.SetTextures(surface);
            drawStep.Effect.Emissive = 0;
            drawStep.Effect.Color = Color.White.ToVector3();
            drawStep.Effect.Instancing = false;

            float phi = (1 + -0.1f * rot.X) % MathHelper.TwoPi;
            float heading = phi + MathHelper.Pi;

            Vector3 position = 100 * new Vector3((float)Math.Cos(phi), (float)Math.Sin(phi), 0) + 10 * Vector3.UnitZ;

            drawStep.Effect.PrepModel(ship);

            Matrix world = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationZ(heading) * Matrix.CreateTranslation(position);

            ship.Draw(world,
                      drawStep.Camera.View,
                      drawStep.Camera.Projection);
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

        private bool KeyReleased(Keys key)
        {
            return Kbrd.KeyReleased(key);
        }

    }
}
