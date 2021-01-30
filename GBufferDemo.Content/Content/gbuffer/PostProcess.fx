#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_3_0
#define PSMODEL ps_3_0
#endif

#include "averageLuminance.hlsl"

Texture2D ColorTexture;
Texture2D BloomTexture;

float GammaReciprocal;

float3 MiddleGrey;
float3 LumWhiteSqr;

float BaseIntensity;
float BaseSaturation;

float BloomIntensity;
float BloomSaturation;

//////////////////////////////////////////////////////////////////////

struct FullScreen_VertexShaderInput
{
    float4 Position : POSITION;
};


//////////////////////////////////////////////////////////////////////

struct FullScreen_PixelShaderInput
{
    float4 Position : SV_POSITION;
    float2 TexCoords : TEXCOORD0;
};

sampler ColorSampler = sampler_state
{
    Texture = <ColorTexture>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler BloomSampler = sampler_state
{
    Texture = <BloomTexture>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

//////////////////////////////////////////////////////////////////////
////  Standard Vertex Shader
//////////////////////////////////////////////////////////////////////

FullScreen_PixelShaderInput vs_FullScreen(FullScreen_VertexShaderInput input)
{
    FullScreen_PixelShaderInput output;
    
    output.Position = input.Position;
    output.TexCoords.x = (1 + input.Position.x) / 2;
    output.TexCoords.y = (1 - input.Position.y) / 2;

    return output;
}

//////////////////////////////////////////////////////////////////////
////  Pixel Shaders
//////////////////////////////////////////////////////////////////////

// Helper for modifying the saturation of a color.
float3 adjustSaturation(float3 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, LUM_FACTOR.rgb);
    float3 grey3 = float3(grey, grey, grey);

    return lerp(grey3, color, saturation);
}


// Gotten from HLSL Development Cookbook by Doron Feinstein
float3 toneMap_hlslbook(float3 color, float avgLum)
{
    float lScale = dot(color, LUM_FACTOR.xyz);
    
    lScale *= MiddleGrey / avgLum;
    lScale = (lScale + lScale * lScale / LumWhiteSqr) / (1.0 + lScale);
    
    return color * lScale;
}

// Reinhard's tone mapping, from this website:
// https://expf.wordpress.com/2010/05/04/reinhards_tone_mapping_operator/
float3 toneMap_logs(float3 color, float avgLum)
{
    float pixelLum = dot(color, LUM_FACTOR.xyz);
    float logPixelLum = log(pixelLum);

    float logAvgLum = log(1 + avgLum);
    float L = logPixelLum / logAvgLum;
    
    return color * (1 + (L * L) / LumWhiteSqr) / (1 + logPixelLum);
}

float3 toneMap_hlslbook_fixed(float3 color, float avgLum)
{
    float pixelLum = dot(color, LUM_FACTOR.xyz);
    
    float lumScale = MiddleGrey / avgLum;
    float pixelScale = (1 + lumScale * lumScale / LumWhiteSqr) / (1.0 + lumScale);
    
    float3 result = color * pixelScale;
    
    return result;
}

float4 toneMap(float3 hdrColor, float avgLum)
{
    float3 ldrColor = toneMap_hlslbook_fixed(hdrColor, avgLum);
    
    float4 screenColor = float4(pow(ldrColor, GammaReciprocal), 1);
    
    return screenColor;
}

//float3 calcBloom(float4 baseColor, float4 bloomColor, float avgLum)
//{
//    float pixelLum = dot(hdrColor, LUM_FACTOR.xyz);
    
//    float threshold = avgLum * 3 + 50;
    
//    float bloomFactor = (pixelLum - threshold) / (1 + threshold);
    
//    bloomFactor = 4 * saturate(bloomFactor / 4);
    
//    float3 bloomColor = hdrColor * bloomFactor;
    
//    return bloomColor;
//}


float3 calcBloom(float3 base, float3 bloom, float avgLum)
{
    float bloomLum = dot(bloom, LUM_FACTOR.xyz);
    
    // Adjust color saturation and intensity.
    float3 sbase = adjustSaturation(base, BaseSaturation) * BaseIntensity;
    float3 sbloom = adjustSaturation(bloom, BloomSaturation) * BloomIntensity;

    // Darken down the base image in areas where there is a lot of bloom,
    // to prevent things looking excessively burned-out.
    sbase *= (1 - saturate(sbloom));

    // Combine the two images.
    return sbase + sbloom;
}


float4 ps_Final(FullScreen_PixelShaderInput input) : SV_Target
{
    float3 hdrColor = ColorTexture.Sample(ColorSampler, input.TexCoords).xyz;
    float avgLum = avgLuminance(input.TexCoords);
        
    return toneMap(hdrColor, avgLum);
}

float4 ps_FinalBloom(FullScreen_PixelShaderInput input) : SV_Target
{
    // Look up the bloom and original base image colors.
    float3 baseColor = ColorTexture.Sample(ColorSampler, input.TexCoords).xyz;
    float3 bloomColor = BloomTexture.Sample(BloomSampler, input.TexCoords).xyz;
    float avgLum = avgLuminance(input.TexCoords);

    float3 hdrColor = calcBloom(baseColor, bloomColor, avgLum);
    
    return toneMap(hdrColor, avgLum);
}


//////////////////////////////////////////////////////////////////////
////  Techniques
//////////////////////////////////////////////////////////////////////

technique Final
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_Final();
    }
}

technique FinalBloom
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_FinalBloom();
    }
}
