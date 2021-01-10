﻿#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_3_0
#define PSMODEL ps_3_0
#endif

// Pixel shader applies a one dimensional gaussian blur filter.
// This is used twice by the bloom postprocess, first to
// blur horizontally, and then again to blur vertically.

#include "fullscreen.hlsl"

texture ColorTexture;

#define SAMPLE_COUNT 15

float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];


sampler TextureSampler = sampler_state
{
    Texture = <ColorTexture>;
};


float4 ps_GaussianBlur(FullScreen_PixelShaderInput input) : COLOR0
{
    float4 c = 0;

    // Combine a number of weighted image filter taps.
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        c += tex2D(TextureSampler, input.TexCoords + SampleOffsets[i]) * SampleWeights[i];
    }

    return c;
}


technique GaussianBlur
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_GaussianBlur();
    }
}