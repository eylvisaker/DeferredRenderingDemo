#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_4_0
#define PSMODEL ps_4_0
#endif

float2 shadowMapSize;

#include "unpackGBuffer.hlsl"
#include "fullscreen.hlsl"


//////////////////////////////////////////////////////////////////////

static const int NumCascades = 4;

// Parameters.

matrix World;
matrix ViewProjection;

float3 CameraPosWS;
matrix ShadowMatrix;
float4 CascadeSplits;
float4 CascadeOffsets[NumCascades];
float4 CascadeScales[NumCascades];

float3 LightDirection;
float3 LightColor;

float Bias;
float OffsetScale;

// Resources.

Texture2DArray ShadowMaps : register(t4);

SamplerComparisonState ShadowSampler : register(s4);

//#if HLSL
//SamplerComparisonState ShadowSampler : register(s4);
//#else
//SamplerState ShadowSampler : register(s4);
//#endif


float ShadowMapSampleCmpLevelZero(float2 uv, uint cascadeIdx, float z)
{ 
    return ShadowMaps.SampleCmpLevelZero(ShadowSampler, float3(uv, cascadeIdx), z);
    
//#if HLSL
//    return ShadowMaps.SampleCmpLevelZero(ShadowSampler, float3(uv, cascadeIdx), z);
//#else
//    return z <= ShadowMaps.SampleLevel(ShadowSampler, float3(uv, cascadeIdx), 0);
//#endif
}

// Pixel shader.
float SampleShadowMap(
    float2 baseUv, float u, float v, float2 shadowMapSizeInv,
    uint cascadeIdx, float depth)
{
    float2 uv = baseUv + float2(u, v) * shadowMapSizeInv;
    float z = depth;

    return ShadowMapSampleCmpLevelZero(uv, cascadeIdx, z);
}

float SampleShadowMapOptimizedPCF(float3 shadowPos,
    float3 shadowPosDX, float3 shadowPosDY,
    uint cascadeIdx, uint filterSize)
{
    float numSlices;

    float lightDepth = shadowPos.z;

    const float bias = Bias;

    lightDepth -= bias;

    float2 uv = shadowPos.xy * shadowMapSize; // 1 unit - 1 texel

    float2 shadowMapSizeInv = 1.0 / shadowMapSize;

    float2 baseUv;
    baseUv.x = floor(uv.x + 0.5);
    baseUv.y = floor(uv.y + 0.5);

    float s = (uv.x + 0.5 - baseUv.x);
    float t = (uv.y + 0.5 - baseUv.y);

    baseUv -= float2(0.5, 0.5);
    baseUv *= shadowMapSizeInv;

    float sum = 0;

    if (filterSize == 2)
    {
        return ShadowMapSampleCmpLevelZero(shadowPos.xy, cascadeIdx, lightDepth);
    }
    else if (filterSize == 3)
    {
        float uw0 = (3 - 2 * s);
        float uw1 = (1 + 2 * s);

        float u0 = (2 - s) / uw0 - 1;
        float u1 = s / uw1 + 1;

        float vw0 = (3 - 2 * t);
        float vw1 = (1 + 2 * t);

        float v0 = (2 - t) / vw0 - 1;
        float v1 = t / vw1 + 1;

        sum += uw0 * vw0 * SampleShadowMap(baseUv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw0 * SampleShadowMap(baseUv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw0 * vw1 * SampleShadowMap(baseUv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw1 * SampleShadowMap(baseUv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth);

        return sum * 1.0f / 16;
    }
    else if (filterSize == 5)
    {
        float uw0 = (4 - 3 * s);
        float uw1 = 7;
        float uw2 = (1 + 3 * s);

        float u0 = (3 - 2 * s) / uw0 - 2;
        float u1 = (3 + s) / uw1;
        float u2 = s / uw2 + 2;

        float vw0 = (4 - 3 * t);
        float vw1 = 7;
        float vw2 = (1 + 3 * t);

        float v0 = (3 - 2 * t) / vw0 - 2;
        float v1 = (3 + t) / vw1;
        float v2 = t / vw2 + 2;

        sum += uw0 * vw0 * SampleShadowMap(baseUv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw0 * SampleShadowMap(baseUv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw0 * SampleShadowMap(baseUv, u2, v0, shadowMapSizeInv, cascadeIdx, lightDepth);

        sum += uw0 * vw1 * SampleShadowMap(baseUv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw1 * SampleShadowMap(baseUv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw1 * SampleShadowMap(baseUv, u2, v1, shadowMapSizeInv, cascadeIdx, lightDepth);

        sum += uw0 * vw2 * SampleShadowMap(baseUv, u0, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw2 * SampleShadowMap(baseUv, u1, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw2 * SampleShadowMap(baseUv, u2, v2, shadowMapSizeInv, cascadeIdx, lightDepth);

        return sum * 1.0f / 144;
    }
    else // filterSize == 7
    {
        float uw0 = (5 * s - 6);
        float uw1 = (11 * s - 28);
        float uw2 = -(11 * s + 17);
        float uw3 = -(5 * s + 1);

        float u0 = (4 * s - 5) / uw0 - 3;
        float u1 = (4 * s - 16) / uw1 - 1;
        float u2 = -(7 * s + 5) / uw2 + 1;
        float u3 = -s / uw3 + 3;

        float vw0 = (5 * t - 6);
        float vw1 = (11 * t - 28);
        float vw2 = -(11 * t + 17);
        float vw3 = -(5 * t + 1);

        float v0 = (4 * t - 5) / vw0 - 3;
        float v1 = (4 * t - 16) / vw1 - 1;
        float v2 = -(7 * t + 5) / vw2 + 1;
        float v3 = -t / vw3 + 3;

        sum += uw0 * vw0 * SampleShadowMap(baseUv, u0, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw0 * SampleShadowMap(baseUv, u1, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw0 * SampleShadowMap(baseUv, u2, v0, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw3 * vw0 * SampleShadowMap(baseUv, u3, v0, shadowMapSizeInv, cascadeIdx, lightDepth);

        sum += uw0 * vw1 * SampleShadowMap(baseUv, u0, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw1 * SampleShadowMap(baseUv, u1, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw1 * SampleShadowMap(baseUv, u2, v1, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw3 * vw1 * SampleShadowMap(baseUv, u3, v1, shadowMapSizeInv, cascadeIdx, lightDepth);

        sum += uw0 * vw2 * SampleShadowMap(baseUv, u0, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw2 * SampleShadowMap(baseUv, u1, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw2 * SampleShadowMap(baseUv, u2, v2, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw3 * vw2 * SampleShadowMap(baseUv, u3, v2, shadowMapSizeInv, cascadeIdx, lightDepth);

        sum += uw0 * vw3 * SampleShadowMap(baseUv, u0, v3, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw1 * vw3 * SampleShadowMap(baseUv, u1, v3, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw2 * vw3 * SampleShadowMap(baseUv, u2, v3, shadowMapSizeInv, cascadeIdx, lightDepth);
        sum += uw3 * vw3 * SampleShadowMap(baseUv, u3, v3, shadowMapSizeInv, cascadeIdx, lightDepth);

        return sum * 1.0f / 2704;
    }
}

float3 SampleShadowCascade(
    float3 shadowPosition, 
    float3 shadowPosDX, float3 shadowPosDY,
    uint cascadeIdx, uint2 screenPos,
    bool visualizeCascades,
    uint filterSize)
{
    shadowPosition += CascadeOffsets[cascadeIdx].xyz;
    shadowPosition *= CascadeScales[cascadeIdx].xyz;

    shadowPosDX *= CascadeScales[cascadeIdx].xyz;
    shadowPosDY *= CascadeScales[cascadeIdx].xyz;

    float3 cascadeColor = float3(1.0f, 1.0f, 1.0f);

    if (visualizeCascades)
    {
        const float3 CascadeColors[NumCascades] =
        {
            float3(1.0f, 0.0f, 0.0f),
            float3(0.0f, 1.0f, 0.0f),
            float3(0.0f, 0.0f, 1.0f),
            float3(1.0f, 1.0f, 0.0f)
        };

        cascadeColor = CascadeColors[cascadeIdx];
    }

    // TODO: Other shadow map modes.

    float shadow = SampleShadowMapOptimizedPCF(shadowPosition, shadowPosDX, shadowPosDY, cascadeIdx, filterSize);

    return shadow * cascadeColor;
}

float3 GetShadowPosOffset(float nDotL, float3 normal)
{
    float numSlices;

    float texelSize = 2.0f / shadowMapSize.x;
    float nmlOffsetScale = saturate(1.0f - nDotL);
    return texelSize * OffsetScale * nmlOffsetScale * normal;
}

float3 ShadowVisibility(
    float3 positionWS, float depthVS, float nDotL, 
    float3 normal, uint2 screenPos, 
    bool filterAcrossCascades,
    bool visualizeCascades,
    uint filterSize)
{
    float3 shadowVisibility = 1.0f;
    int cascadeIdx = 0;

    // Figure out which cascade to sample from.
    [unroll]
    for (int i = 0; i < NumCascades - 1; ++i)
    {
        [flatten]
        if (depthVS > CascadeSplits[i])
            cascadeIdx = i + 1;
    }

    // Apply offset
    float3 offset = GetShadowPosOffset(nDotL, normal) / abs(CascadeScales[cascadeIdx].z);

    // Project into shadow space
    float3 samplePos = positionWS + offset;
    float3 shadowPosition = mul(float4(samplePos, 1.0f), ShadowMatrix).xyz;
    float3 shadowPosDX = ddx_fine(shadowPosition);
    float3 shadowPosDY = ddy_fine(shadowPosition);

    shadowVisibility = SampleShadowCascade(shadowPosition, 
        shadowPosDX, shadowPosDY, cascadeIdx, screenPos,
        visualizeCascades, filterSize);

    if (filterAcrossCascades)
    {
        // Sample the next cascade, and blend between the two results to
        // smooth the transition
        const float BlendThreshold = 0.1f;
        float nextSplit = CascadeSplits[cascadeIdx];
        float splitSize = cascadeIdx == 0 ? nextSplit : nextSplit - CascadeSplits[cascadeIdx - 1];
        float splitDist = (nextSplit - depthVS) / splitSize;

        [branch]
        if (splitDist <= BlendThreshold && cascadeIdx != NumCascades - 1)
        {
            float3 nextSplitVisibility = SampleShadowCascade(shadowPosition,
                shadowPosDX, shadowPosDY, cascadeIdx + 1, screenPos,
                visualizeCascades, filterSize);
            float lerpAmt = smoothstep(0.0f, BlendThreshold, splitDist);
            shadowVisibility = lerp(nextSplitVisibility, shadowVisibility, lerpAmt);
        }
    }

    return shadowVisibility;
}

float3 calcDirectional(float3 position, Material material)
{
    // Phong diffuse
    float NDotL = dot(LightDirection, material.normal);
    float3 finalColor = LightColor.rgb * saturate(NDotL);

    // Blinn specular
    float3 toEye = CameraPosWS - position;
    toEye = normalize(toEye);
    float3 halfWay = normalize(toEye + LightDirection);
    float NDotH = saturate(dot(halfWay, material.normal));
    
    // prevent bleed-through. If NDotL is negative, then the surface is
    // facing away from the light and shouldn't have specular lighting. 
    // But NDotH can still be positive.
    NDotH *= saturate(NDotL * 10);
     
    finalColor += LightColor.rgb * pow(NDotH, material.specExp) * material.specIntensity;
    
    return finalColor * material.diffuseColor.rgb;
}

float4 ps_DirectionShadow(FullScreen_PixelShaderInput input,
    bool visualizeCascades, bool filterAcrossCascades, 
    uint filterSize)
{
    Surface surface = unpackGBuffer(input.TexCoords);
    Material mat = createMaterial(surface);
    
    float3 position = surface.worldPos;
    float3 normalWS = mat.normal;
    float3 diffuseAlbedo = mat.diffuseColor.rgb;
    
    float depthVS = surface.depthVS;
    
    float nDotL = saturate(dot(normalWS, LightDirection));
    uint2 screenPos = uint2(0, 0); // apparently screenPos is unused??
    float3 shadowVisibility = ShadowVisibility(
        position, depthVS, nDotL, normalWS, screenPos,
        filterAcrossCascades, visualizeCascades, filterSize);
    
    float3 lighting = calcDirectional(position, mat);

    // Add the directional light.
    lighting *= shadowVisibility;
    
    return float4(lighting, 1);
}



float4 ps_VisualizeFalseFilterFalseFilterSizeFilter2x2(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, false, false, 2);
}

float4 ps_VisualizeTrueFilterFalseFilterSizeFilter2x2(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, true, false, 2);
}

float4 ps_VisualizeFalseFilterFalseFilterSizeFilter3x3(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, false, false, 3);
}

float4 ps_VisualizeTrueFilterFalseFilterSizeFilter3x3(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, true, false, 3);
}

float4 ps_VisualizeFalseFilterFalseFilterSizeFilter5x5(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, false, false, 5);
}

float4 ps_VisualizeTrueFilterFalseFilterSizeFilter5x5(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, true, false, 5);
}

float4 ps_VisualizeFalseFilterFalseFilterSizeFilter7x7(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, false, false, 7);
}

float4 ps_VisualizeTrueFilterFalseFilterSizeFilter7x7(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, true, false, 7);
}

float4 ps_VisualizeFalseFilterTrueFilterSizeFilter2x2(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, false, true, 2);
}

float4 ps_VisualizeTrueFilterTrueFilterSizeFilter2x2(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, true, true, 2);
}

float4 ps_VisualizeFalseFilterTrueFilterSizeFilter3x3(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, false, true, 3);
}

float4 ps_VisualizeTrueFilterTrueFilterSizeFilter3x3(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, true, true, 3);
}

float4 ps_VisualizeFalseFilterTrueFilterSizeFilter5x5(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, false, true, 5);
}

float4 ps_VisualizeTrueFilterTrueFilterSizeFilter5x5(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, true, true, 5);
}

float4 ps_VisualizeFalseFilterTrueFilterSizeFilter7x7(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, false, true, 7);
}

float4 ps_VisualizeTrueFilterTrueFilterSizeFilter7x7(FullScreen_PixelShaderInput input) : SV_Target
{
    return ps_DirectionShadow(input, true, true, 7);
}

// Techniques.

technique VisualizeFalseFilterFalseFilterSizeFilter2x2
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeFalseFilterFalseFilterSizeFilter2x2();
    }
}

technique VisualizeTrueFilterFalseFilterSizeFilter2x2
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeTrueFilterFalseFilterSizeFilter2x2();
    }
}

technique VisualizeFalseFilterFalseFilterSizeFilter3x3
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeFalseFilterFalseFilterSizeFilter3x3();
    }
}

technique VisualizeTrueFilterFalseFilterSizeFilter3x3
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeTrueFilterFalseFilterSizeFilter3x3();
    }
}

technique VisualizeFalseFilterFalseFilterSizeFilter5x5
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeFalseFilterFalseFilterSizeFilter5x5();
    }
}

technique VisualizeTrueFilterFalseFilterSizeFilter5x5
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeTrueFilterFalseFilterSizeFilter5x5();
    }
}

technique VisualizeFalseFilterFalseFilterSizeFilter7x7
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeFalseFilterFalseFilterSizeFilter7x7();
    }
}

technique VisualizeTrueFilterFalseFilterSizeFilter7x7
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeTrueFilterFalseFilterSizeFilter7x7();
    }
}

technique VisualizeFalseFilterTrueFilterSizeFilter2x2
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeFalseFilterTrueFilterSizeFilter2x2();
    }
}

technique VisualizeTrueFilterTrueFilterSizeFilter2x2
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeTrueFilterTrueFilterSizeFilter2x2();
    }
}

technique VisualizeFalseFilterTrueFilterSizeFilter3x3
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeFalseFilterTrueFilterSizeFilter3x3();
    }
}

technique VisualizeTrueFilterTrueFilterSizeFilter3x3
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeTrueFilterTrueFilterSizeFilter3x3();
    }
}

technique VisualizeFalseFilterTrueFilterSizeFilter5x5
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeFalseFilterTrueFilterSizeFilter5x5();
    }
}

technique VisualizeTrueFilterTrueFilterSizeFilter5x5
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeTrueFilterTrueFilterSizeFilter5x5();
    }
}

technique VisualizeFalseFilterTrueFilterSizeFilter7x7
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeFalseFilterTrueFilterSizeFilter7x7();
    }
}

technique VisualizeTrueFilterTrueFilterSizeFilter7x7
{
    pass
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_VisualizeTrueFilterTrueFilterSizeFilter7x7();
    }
}
