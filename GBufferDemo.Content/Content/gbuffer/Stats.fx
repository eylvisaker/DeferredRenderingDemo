#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_3_0
#define PSMODEL ps_3_0
#endif

#include "fullscreen.hlsl"

Texture2D A_Texture;
Texture2D B_Texture;
float4 A_Weight;
float4 B_Weight;
float2 HalfTexel;

sampler A_Sampler = sampler_state
{
    Texture = <A_Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = point;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler B_Sampler = sampler_state
{
    Texture = <B_Texture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = point;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
};


//////////////////////////////////////////////////////////////////////
////  Standard Vertex Shader
//////////////////////////////////////////////////////////////////////

FullScreen_PixelShaderInput vs_FullScreenTexelOffset(FullScreen_VertexShaderInput input)
{
    FullScreen_PixelShaderInput output;
    
    output.Position = input.Position;
    output.TexCoords.x = (1 + input.Position.x) / 2;
    output.TexCoords.y = (1 - input.Position.y) / 2;

    output.TexCoords += HalfTexel;
    
    return output;
}

//////////////////////////////////////////////////////////////////////
////  Pixel Shaders
//////////////////////////////////////////////////////////////////////

float4 ps_average(FullScreen_PixelShaderInput input) : SV_Target
{
    float4 a = A_Texture.Sample(A_Sampler, input.TexCoords);
    float4 b = B_Texture.Sample(B_Sampler, input.TexCoords);

    return A_Weight * a + B_Weight * b;
}

//////////////////////////////////////////////////////////////////////
////  Techniques
//////////////////////////////////////////////////////////////////////

technique Average
{
    pass P0
    {
        VertexShader = compile VSMODEL vs_FullScreenTexelOffset();
        PixelShader = compile PSMODEL ps_average();
    }
};