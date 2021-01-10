using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GBufferDemoLib
{
    internal class Skybox
    {
        private readonly GraphicsDevice graphics;
        private ContentManager content;
        private Texture2D texture;
        private Model model;
        private FillGBufferEffect effect;

        public Skybox(GraphicsDevice graphics, ContentManager content)
        {
            this.graphics = graphics;
            this.content = content;

            texture = content.Load<Texture2D>("skybox_texture");
            model = content.Load<Model>("skybox");
        }

        public FillGBufferEffect Effect
        {
            get => effect; set
            {
                effect = value;

                foreach (var mesh in model.Meshes)
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        part.Effect = Effect;
                    }
                }
            }
        }

        public void Draw(FillGBufferEffect gEffect)
        {
        }

        internal void Draw(Vector3 position)
        {
            graphics.RasterizerState = RasterizerState.CullNone;

            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = Effect;
                }
            }

            Effect.DiffuseTexture = texture;
            Effect.CurrentTechnique = Effect.TechniqueSprite;
            Effect.World = Matrix.CreateRotationX(MathHelper.PiOver2)
                         * Matrix.CreateScale(400);

            foreach (var mesh in model.Meshes)
            {
                mesh.Draw();
            }
        }
    }
}