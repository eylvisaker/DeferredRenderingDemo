#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_4_0
#define PSMODEL ps_4_0
#endif

// Pixel shader filters the input texture so that only the really bright pixels are brought through.

#include "fullscreen.hlsl"
#include "averageLuminance.hlsl"

float BloomMinThreshold;

Texture2D ColorTexture;

sampler ColorSampler = sampler_state
{
    Texture = <ColorTexture>;
};

float4 ps_BloomFilter(FullScreen_PixelShaderInput input) : SV_TARGET0
{
    float3 baseColor = ColorTexture.Sample(ColorSampler, input.TexCoords).xyz;
    float avgLum = avgLuminance(input.TexCoords);
    
    float threshold = avgLum * 3 + BloomMinThreshold;
    float lum = dot(baseColor, LUM_FACTOR.rgb);
    
    baseColor *= saturate((lum - threshold) / (0.5 + BloomMinThreshold));
    
    return float4(baseColor, 1);
}


technique GaussianBlur
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_BloomFilter();
    }
}
