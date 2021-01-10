using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemoLib.GBuffers.Effects
{
    public static class EffectExtensions
    {
        /// <summary>
        /// Prepares a model to be rendered using the associated effect
        /// </summary>
        /// <param name="model"></param>
        public static void PrepModel(this Effect effect, Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }
        }
    }
}
