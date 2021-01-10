#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_3_0
#define PSMODEL ps_3_0
#endif

#include "unpackGBuffer.hlsl"

texture BackgroundTexture;

float4x4 WorldViewProjection;
float4 Color;

//////////////////////////////////////////////////////////////////////

struct Background_VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
};

//////////////////////////////////////////////////////////////////////

struct Background_PixelShaderInput
{
    float4 Position : SV_POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 ScreenPosition : TEXCOORD1;
};

sampler BackgroundSampler = sampler_state
{
    Texture = <BackgroundTexture>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

//////////////////////////////////////////////////////////////////////
////  Standard Vertex Shader
//////////////////////////////////////////////////////////////////////

Background_PixelShaderInput vs_Background(Background_VertexShaderInput input)
{
    Background_PixelShaderInput output;
    
    float4 pos = mul(input.Position, WorldViewProjection);
    
    output.Position = pos;
    output.TexCoords = input.TexCoords;
    output.ScreenPosition = pos;
    
    return output;
}


//////////////////////////////////////////////////////////////////////
////  Pixel Shaders
//////////////////////////////////////////////////////////////////////

float4 ps_Background(Background_PixelShaderInput input) : COLOR
{
    float2 screenPos = input.ScreenPosition.xy / input.ScreenPosition.ww;
    float2 texCoord = 0.5 * (float2(screenPos.x, -screenPos.y) + 1);
    
    Surface surface = unpackGBuffer(texCoord);
    
    // background is at depth = 1.
    if (surface.depth < 1)
        discard;

    float4 color = tex2D(BackgroundSampler, input.TexCoords);
    float3 linearColor = pow(color.rgb, Gamma);
    
    return float4(linearColor * Color.rgb, saturate(color.a) * Color.a);
}

//////////////////////////////////////////////////////////////////////
////  Techniques
//////////////////////////////////////////////////////////////////////

technique Background
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_Background();
        PixelShader = compile PSMODEL ps_Background();
    }
}