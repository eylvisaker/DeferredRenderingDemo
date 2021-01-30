using GBufferDemo.Cameras;
using GBufferDemo.GBuffers;
using GBufferDemo.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace GBufferDemo.Scenes.Village
{
    public class VillageScene : IScene
    {
        private SpriteBatch _spriteBatch;

        private FirstPersonCamera camera;
        private Model _model;
        private Texture2D white;
        private Matrix[] _modelTransforms;
        private GraphicsDevice graphics;
        private ContentManager content;
        private List<PointLight> lights = new List<PointLight>();

        public Camera Camera => camera;

        public Player Player { get; private set; } = new Player();

        public IReadOnlyList<PointLight> Lights => lights;

        //private GameSettingsComponent _gameSettings;

        //private MeshRenderer _meshRenderer;

        public VillageScene(GraphicsDevice graphics, ContentManager content)
        {
            this.graphics = graphics;
            this.content = content;

            Initialize();
        }

        protected void Initialize()
        {
            camera = new FirstPersonCamera(MathHelper.PiOver4 * 0.75f, graphics.Viewport.AspectRatio, 0.25f, 250.0f);

            camera.Position = new Vector3(-111, -46, 0);
            camera.XRotation = -MathHelper.PiOver2;
            camera.YRotation = -MathHelper.Pi;

            camera.SetLookAt(camera.Position, camera.Position + new Vector3(6, 8, 0), Vector3.UnitZ);

            //Components.Add(new FramesPerSecondComponent(this));
            //Components.Add(_gameSettings = new GameSettingsComponent(this));

            //var guiService = new GuiComponent(this);
            //Components.Add(guiService);
            //Services.AddService<IGuiService>(guiService);

            LoadContent();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected void LoadContent()
        {
            _spriteBatch = new SpriteBatch(graphics);

            _model = content.Load<Model>("village/models/village_house_fbx");
            white = content.Load<Texture2D>("white");

            _modelTransforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_modelTransforms);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            // Apply keyboard input.

            var keyboardState = Keyboard.GetState();
            var gamePadState = GamePad.GetState(PlayerIndex.One);

            var deltaMilliseconds = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            float moveSpeed = 0.025f * deltaMilliseconds;
            float lookSpeed = moveSpeed * 0.1f;

            if (keyboardState.IsKeyDown(Keys.LeftShift))
                moveSpeed *= 0.25f;

            var cameraPosition = camera.Position;
            if (keyboardState.IsKeyDown(Keys.W))
                cameraPosition += camera.Forward * moveSpeed;
            else if (keyboardState.IsKeyDown(Keys.S))
                cameraPosition += camera.Backward * moveSpeed;
            if (keyboardState.IsKeyDown(Keys.A))
                cameraPosition += camera.Left * moveSpeed;
            else if (keyboardState.IsKeyDown(Keys.D))
                cameraPosition += camera.Right * moveSpeed;
            if (keyboardState.IsKeyDown(Keys.Q))
                cameraPosition += camera.Up * moveSpeed;
            else if (keyboardState.IsKeyDown(Keys.E))
                cameraPosition += camera.Down * moveSpeed;

            cameraPosition += camera.Forward * moveSpeed * gamePadState.ThumbSticks.Left.Y;
            cameraPosition += camera.Right * moveSpeed * gamePadState.ThumbSticks.Left.X;

            cameraPosition += camera.Up * moveSpeed * (gamePadState.Triggers.Right - gamePadState.Triggers.Left);

            camera.Position = cameraPosition;

            var target = camera.Position + camera.Forward;

            target += camera.Right * gamePadState.ThumbSticks.Right.X * lookSpeed;
            target += camera.Up * gamePadState.ThumbSticks.Right.Y * lookSpeed * 0.5f;

            camera.SetLookAt(cameraPosition, target, Vector3.UnitZ);

            Player.Position = cameraPosition;
            Player.Facing = camera.Forward;
        }

        public void Draw(DrawStep drawStep)
        {
            Matrix worldMatrix = Matrix.CreateRotationX(MathHelper.PiOver2);

            // Draw all meshes.
            foreach (var mesh in _model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts.Where(mp => mp.PrimitiveCount > 0))
                {
                    var basicEffect = (BasicEffect)meshPart.Effect;

                    drawStep.Effect.Color = basicEffect.DiffuseColor;
                    drawStep.Effect.World = _modelTransforms[mesh.ParentBone.Index] * worldMatrix;
                    drawStep.Effect.SetTextures(white);

                    foreach (var pass in drawStep.Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        graphics.SetVertexBuffer(meshPart.VertexBuffer);
                        graphics.Indices = meshPart.IndexBuffer;

                        graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                            meshPart.VertexOffset, 0, meshPart.PrimitiveCount);
                    }
                }
            }
        }
    }
}