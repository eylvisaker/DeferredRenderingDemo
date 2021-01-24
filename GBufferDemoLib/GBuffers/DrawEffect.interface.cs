using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBufferDemoLib.GBuffers
{
    public interface IDrawEffect : IEffectMatrices
    {
        Vector3 Color { get; set; }
        float SpecularExponent { get; set; }
        float SpecularIntensity { get; set; }
        float Emissive { get; set; }

        EffectTechnique CurrentTechnique { get; }

        bool Instancing { get; set; }

        Effect AsEffect();

        void SetTextures(Texture2D diffuse,
                         Texture2D normalMap = null,
                         Texture2D specularMap = null);
    }
}