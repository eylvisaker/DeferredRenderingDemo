#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_3_0
#define PSMODEL ps_3_0
#endif

#include "fullscreen.hlsl"

Texture2D ColorTexture;

//////////////////////////////////////////////////////////////////////

sampler ColorSampler = sampler_state
{
    Texture = <ColorTexture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = point;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

//////////////////////////////////////////////////////////////////////
////  Pixel Shaders
//////////////////////////////////////////////////////////////////////

float4 ps_Final(FullScreen_PixelShaderInput input) : SV_Target
{
    float4 result = ColorTexture.Sample(ColorSampler, input.TexCoords);
    
    return result;
}

//////////////////////////////////////////////////////////////////////
////  Techniques
//////////////////////////////////////////////////////////////////////

technique Downscale
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_Final();
    }
}
