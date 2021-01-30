
float Gamma;

float4x4 ViewProjectionInv;

Texture2D ColorTexture;
Texture2D DepthTexture;
Texture2D NormalTexture;
Texture2D SpecularTexture;

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
    float depthVS;
    float3 color;
    float alpha;
    float emissive;
    float3 normal;
    float specInt;
    float specPow;
    float3 worldPos;
};

struct Material
{
    float3 normal;
    float4 diffuseColor;
    float emissive;
    float specExp;
    float specIntensity;
};

Surface unpackGBuffer(float2 texCoords)
{
    Surface result;
    
    float4 color = ColorTexture.Sample(ColorSampler, texCoords);
    float depth = 1 - DepthTexture.Sample(DepthSampler, texCoords).x;
    float3 normal = NormalTexture.Sample(NormalSampler, texCoords).xyz;
    float2 specular = SpecularTexture.Sample(SpecularSampler, texCoords).xy;
    
    // Unpack the emissive value, so that the lowest fifty values
    // exist on a fine-grain scale but everything above that is coarser 
    // grain, up to an emissive value of 2 at color.a = 1.
    // First scale emissive to integer values from 0-255 (byte storage);
    float emissive = color.a * 255;
    
    // Next scale emissive to 0-1000
    emissive += saturate(emissive - 50) * 4.634;  
    
    // Now scale emissive to actual HDR range we want to use. 
    // 0.005 puts it from the range of 0-5
    emissive *= 0.002;
    
    result.color = pow(color.xyz, Gamma);
    result.emissive = emissive;
    result.depth = depth;
    result.normal = normal * 2 - 1;
    result.specPow = specular.x;
    result.specInt = specular.y;
    
    float4 position;
    
    position.x = texCoords.x * 2 - 1;
    position.y = -(texCoords.y * 2 - 1);
    position.z = depth;
    position.w = 1.0;
    
    float4 worldPos = mul(position, ViewProjectionInv);
    
    result.depthVS = 1 / worldPos.w;
    result.worldPos = (worldPos / worldPos.w).xyz;
    
    return result;
}

Material createMaterial(Surface surface)
{
    Material mat;
    
    mat.normal = surface.normal;
    mat.diffuseColor = float4(surface.color.xyz, 1);
    mat.specExp = g_SpecExpRange.x + g_SpecExpRange.y * surface.specPow;
    mat.specIntensity = surface.specInt;
    mat.emissive = surface.emissive;
    
    return mat;
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
