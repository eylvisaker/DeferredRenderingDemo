#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_3_0
#define PSMODEL ps_3_0
#endif

float Gamma;

float4x4 ViewProjectionInv;

texture ColorTexture;
texture DepthTexture;
texture NormalTexture;
texture SpecularTexture;

sampler ColorSampler = sampler_state
{
    Texture = <ColorTexture>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler DepthSampler = sampler_state
{
    Texture = <DepthTexture>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler NormalSampler = sampler_state
{
    Texture = <NormalTexture>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler SpecularSampler = sampler_state
{
    Texture = <SpecularTexture>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

//////////////////////////////////////////////////////////////////////
////  Reading the GBuffer
//////////////////////////////////////////////////////////////////////

// This must match the same named constants in FillGBuffer.fx
static const float2 g_SpecExpRange = { 0.1, 16384.1 };

struct Surface
{
    float depth;
    float3 color;
    float alpha;
    float emissive;
    float3 normal;
    float specInt;
    float specPow;
};

Surface unpackGBuffer(float2 texCoords)
{
    Surface result;
    
    float4 color = tex2D(ColorSampler, texCoords);
    float depth = 1 - tex2D(DepthSampler, texCoords).x;
    float3 normal = tex2D(NormalSampler, texCoords).xyz;
    float2 specular = tex2D(SpecularSampler, texCoords).xy;
    
    // Unpack the emissive value, so that the lowest fifty values
    // exist on a fine-grain scale but everything above that is coarser 
    // grain, up to an emissive value of 1000 at color.a = 1.
    float emissive = color.a * 255;
    emissive += saturate(emissive - 50) * 4.87;  
    
    result.color = pow(color.xyz, Gamma);
    result.emissive = emissive;
    result.depth = depth;
    result.normal = normal * 2 - 1;
    result.specPow = specular.x;
    result.specInt = specular.y;
    
    return result;
}

float3 calcWorldPos(float2 csPos, float depth)
{
    float4 position;
    
    position.x = csPos.x * 2 - 1;
    position.y = -(csPos.y * 2 - 1);
    position.z = depth;
    position.w = 1.0;
    
    float4 result = mul(position, ViewProjectionInv);
    result /= result.w;

    return result.xyz;
}
