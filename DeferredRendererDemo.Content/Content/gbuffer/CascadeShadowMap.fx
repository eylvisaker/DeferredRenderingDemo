#define VSMODEL vs_5_0
#define PSMODEL ps_5_0

matrix WorldViewProjection;

struct VSOutput
{
    float4 position : SV_Position;
    float2 depth : TEXCOORD0;
};

VSOutput vs_ShadowMap(float3 position : POSITION)
{
    VSOutput output;
    output.position = mul(float4(position, 1), WorldViewProjection);
    output.depth = output.position.zw;
    return output;
}

VSOutput vs_ShadowMapInstance(float4 position : POSITION, float4x4 _instanceTransform : BLENDWEIGHT0)
{
    VSOutput output;
    
#if HLSL
    float4x4 instanceTransform = transpose(_instanceTransform);
#else
    float4x4 instanceTransform = _instanceTransform;
#endif
    
    float4 pos = mul(mul(position, instanceTransform), WorldViewProjection);
    
    output.position = pos;
    output.depth = output.position.zw;

    return output;
}

float4 ps_ShadowMap(VSOutput input) : SV_Target
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
