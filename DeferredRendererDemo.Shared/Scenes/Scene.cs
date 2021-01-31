using DeferredRendererDemo.Cameras;
using DeferredRendererDemo.GBuffers;
using DeferredRendererDemo.Lights;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeferredRendererDemo.Scenes
{
    public interface IScene
    {
        Camera Camera { get; }
        Player Player { get; }

        IReadOnlyList<PointLight> Lights { get; }

        void Update(GameTime gameTime);
        void Draw(DrawStep drawStep);
    }
}
