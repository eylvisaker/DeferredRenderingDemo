using GBufferDemo.Cameras;
using GBufferDemo.GBuffers;
using GBufferDemo.Lights;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GBufferDemo.Scenes
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
