#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_3_0
#define PSMODEL ps_3_0
#endif

float4x4 WorldViewProjection;
float4x4 World;
float3 SpriteNormal;

////////////////////////////////
// Desaturation Parameters
////////////////////////////////
int ApplyDesat;
float Desat;
float Devalue;
float PreserveColor;
float PreserveColorAngle;

////////////////////////////////
// Texture Parameters
////////////////////////////////
texture DiffuseTexture;
texture NormalMapTexture;

float Emissive;

////////////////////////////////
// Vertex Inputs
////////////////////////////////
struct VertexShaderInput_Textured
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float3 Normal : NORMAL;
};

struct VertexShaderInput_Bumped
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float3 Normal : NORMAL;
    float3 Tangent0 : TANGENT0;
    float3 Tangent1 : TANGENT1;
};

struct VertexShaderInput_Sprite
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
};

struct VertexShaderInput_ColoredSprite
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

////////////////////////////////
// Internal structures
////////////////////////////////

sampler diffuseSampler = sampler_state
{
    Texture = <DiffuseTexture>;
};
sampler normalSampler = sampler_state
{
    Texture = <NormalMapTexture>;
};

struct PSIN_Textured
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 Normal : NORMAL0;
    float2 TexCoords : TEXCOORD0;
    float2 Depth : TEXCOORD1;
};

struct PSIN_Bumped
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 Normal : NORMAL0;
    float3 Tangent0 : TANGENT0;
    float3 Tangent1 : TANGENT1;
    float2 TexCoords : TEXCOORD0;
    float2 Depth : TEXCOORD1;
};

struct PSOUT_GBuffer
{
    float4 Color : COLOR0;
    float4 Depth : COLOR1;
    float4 Normal : COLOR2;
};

struct PSOUT_Color
{
    float4 Color : COLOR0;
};

/////////////////////////////
//// Functions
/////////////////////////////

static const float Epsilon = 1e-10;

//float3 RGBtoHCV(in float3 RGB)
//{
//    // Based on work by Sam Hocevar and Emil Persson
//    float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
//    float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
//    float C = Q.x - min(Q.w, Q.y);
//    float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
//    return float3(H, C, Q.x);
//}

//float3 RGBtoHSV(in float3 RGB)
//{
//    float3 HCV = RGBtoHCV(RGB);
//    float S = HCV.y / (HCV.z + Epsilon);
//    return float3(HCV.x, S, HCV.z);
//}

//float3 HUEtoRGB(in float H)
//{
//    float R = abs(H * 6 - 3) - 1;
//    float G = 2 - abs(H * 6 - 2);
//    float B = 2 - abs(H * 6 - 4);
//    return saturate(float3(R,G,B));
//}

//float3 HSVtoRGB(in float3 HSV)
//{
//    float3 RGB = HUEtoRGB(HSV.x);
//    return ((RGB - 1) * HSV.y + 1) * HSV.z;
//}

//float colorDist(float h1, float h2)
//{
//    return (abs(h1 - h2) % 1) / PreserveColorAngle;
//}

//float4 desaturate(in float4 texel)
//{
//    float3 hsv = RGBtoHSV(texel.xyz);

//    float accum = 1;
//    accum = accum * saturate(pow(abs(colorDist(PreserveColor, hsv.r)), 100));

//    hsv.y = saturate(hsv.y * (1 - (Desat * accum)));
//    hsv.z = saturate(hsv.z * (1 - (Devalue * accum)));

//    float4 rgba = float4(HSVtoRGB(hsv), texel.a);

//    return rgba;
//}

//////////////////////////////////////////////////////////////////////
////  Standard Vertex Shader
//////////////////////////////////////////////////////////////////////

PSIN_Textured vs_Sprite(VertexShaderInput_Sprite input)
{
    PSIN_Textured output;

    float4 pos = mul(input.Position, WorldViewProjection);
    
    output.Position = pos;
    output.TexCoords = input.TexCoords;
    output.Normal = SpriteNormal;
    output.Color = float4(1, 1, 1, 1);
    output.Depth.x = pos.z;
    output.Depth.y = pos.w;

    return output;
}

PSIN_Textured vs_ColoredSprite(VertexShaderInput_ColoredSprite input)
{
    PSIN_Textured output;

    float4 pos = mul(input.Position, WorldViewProjection);
    
    output.Position = pos;
    output.TexCoords = input.TexCoords;
    output.Normal = SpriteNormal;
    output.Color = input.Color;
    output.Depth.x = pos.z;
    output.Depth.y = pos.w;

    return output;
}

PSIN_Textured vs_Textured(VertexShaderInput_Textured input)
{
    PSIN_Textured output;

    float4 pos = mul(input.Position, WorldViewProjection);
    
    output.Position = pos;
    output.TexCoords = input.TexCoords;
    output.Normal = mul(input.Normal, (float3x3)World);
    output.Color = float4(1, 1, 1, 1);
    output.Depth.x = pos.z;
    output.Depth.y = pos.w;

    return output;
}

PSIN_Bumped vs_Bumped(VertexShaderInput_Bumped input)
{
    PSIN_Bumped output;

    float4 pos = mul(input.Position, WorldViewProjection);
    
    output.Position = pos;
    output.TexCoords = input.TexCoords;
    output.Normal = normalize(mul(input.Normal, (float3x3) World));
    output.Tangent0 = normalize(mul(input.Tangent0, (float3x3) World));
    output.Tangent1 = normalize(mul(input.Tangent1, (float3x3) World));
    output.Color = float4(1, 1, 1, 1);
    output.Depth.x = pos.z;
    output.Depth.y = pos.w;

    return output;
}

//////////////////////////////////////////////////////////////////////
////  Pixel Shaders
//////////////////////////////////////////////////////////////////////

PSOUT_GBuffer packGBuffer(float4 color, float3 normal, float2 depth)
{
    PSOUT_GBuffer result;
    float d = depth.x / depth.y;
    
    result.Color = float4(color.rgb, Emissive);
    result.Depth = d;
    result.Normal = float4(0.5 * (normalize(normal).xyz + 1), 1);

    return result;
}

PSOUT_GBuffer ps_Sprite(PSIN_Textured input)
{
    float4 texel = tex2D(diffuseSampler, input.TexCoords);

    if (texel.a < 0.1)
        discard;

    //if (ApplyDesat)
    //{
    //    texel = desaturate(texel);
    //}

    return packGBuffer(texel * input.Color,
                       input.Normal,
                       input.Depth);
}

PSOUT_GBuffer ps_Textured(PSIN_Textured input)
{
    return ps_Sprite(input);
    //float4 texel = tex2D(diffuseSampler, input.TexCoords);
    //PSOUT_Color result;
    
    //result.Color = texel;
    
    //return result;
}

PSOUT_GBuffer ps_Bumped(PSIN_Bumped input)
{
    float4 texel = tex2D(diffuseSampler, input.TexCoords);

    if (texel.a < 0.1)
        discard;
    
    //if (ApplyDesat)
    //{
    //    texel = desaturate(texel);
    //}
    
    float4 bump = tex2D(normalSampler, input.TexCoords);
    bump = (bump * 2) - 1;
    
    float3 bumpNormal = bump.x * input.Tangent0 
                      + bump.y * input.Tangent1
                      + bump.z * input.Normal;
    
    return packGBuffer(input.Color * texel,
                       bumpNormal,
                       input.Depth);
}

//// // Turns to gray scale
//// PS_GBuffer_OUT ps_GrayScale(PSIN_Textured input) : COLOR0
//// {
////     float4 texel = tex2D(diffuseSampler, input.TexCoords);
////     PS_RenderOutput result;

////     if (texel.a < 0.1)
////         discard;

////     float gray = 0.299 * texel.r + 0.587 * texel.g + 0.114 * texel.b;
////     float3 hsv = RGBtoHSV(texel.rgb);

////     hsv.g *= 1 - Desat;

////     result.Color = float4(HSVtoRGB(hsv), texel.a);

////     return result;
//// }

//// // Turns to gray scale but colored by the input vertices
//// PS_RenderOutput ps_GrayScaleColor(PSIN_Textured input) : COLOR0
//// {
////     float4 texel = tex2D(diffuseSampler, input.TexCoords);
////     PS_RenderOutput result;

////     if (texel.a < 0.1)
////         discard;

////     float gray = 0.299 * texel.r + 0.587 * texel.g + 0.114 * texel.b;
////     float3 hsv = RGBtoHSV(texel.rgb);

////     hsv.g *= 1 - Desat;

////     result.Color = float4(HSVtoRGB(hsv), texel.a)* input.Color;

////     return result;
//// }

//// PS_RenderOutput ps_Texture(PSIN_Textured input) : COLOR0
//// {
////     float4 texel = tex2D(diffuseSampler, input.TexCoords);
////     PS_RenderOutput result;

////     result.Color = texel;
////     return result;
//// }

////////////////////////////////////////////////////////////////////////////
////  Techniques
////////////////////////////////////////////////////////////////////////////

technique Sprite
{
    pass Pass1
    {
        VertexShader = compile VSMODEL vs_Sprite();
        PixelShader = compile PSMODEL ps_Sprite();
    }
}

technique Textured
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_Textured();
        PixelShader = compile PSMODEL ps_Textured();
    }
}

technique Bumped
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_Bumped();
        PixelShader = compile PSMODEL ps_Bumped();
    }
}
