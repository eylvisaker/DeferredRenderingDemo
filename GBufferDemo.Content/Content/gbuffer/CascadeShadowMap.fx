#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_3_0
#define PSMODEL ps_3_0
#endif

matrix WorldViewProjection;

struct VSOutput
{
    float4 position : SV_Position;
    float2 depth : TEXCOORD0;
};

VSOutput vs_ShadowMap(float3 position : SV_Position)
{
    VSOutput output;
    output.position = mul(float4(position, 1), WorldViewProjection);
    output.depth = output.position.zw;
    return output;
}

VSOutput vs_ShadowMapInstance(float4 position : SV_Position, float4x4 instanceTransform : BLENDWEIGHT0)
{
    VSOutput output;
    
    float4 pos = mul(mul(position, transpose(instanceTransform)), WorldViewProjection);
    
    output.position = pos;
    output.depth = output.position.zw;

    return output;
}

float4 ps_ShadowMap(VSOutput input) : COLOR
{
    return float4(input.depth.x / input.depth.y, 0, 0, 1);
}

technique ShadowMap
{
    pass
    {
        VertexShader = compile VSMODEL vs_ShadowMap();
        PixelShader = compile PSMODEL ps_ShadowMap();
    }
}

technique ShadowMapInstance
{
    pass
    {
        VertexShader = compile VSMODEL vs_ShadowMapInstance();
        PixelShader = compile PSMODEL ps_ShadowMap();
    }
}
