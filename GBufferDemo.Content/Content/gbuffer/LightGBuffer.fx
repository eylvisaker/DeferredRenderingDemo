#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_3_0
#define PSMODEL ps_3_0
#endif

#include "unpackGBuffer.hlsl"
#include "fullscreen.hlsl"

float4x4 WorldViewProjection;

float3 EyePosition;

float3 AmbientUpRange;
float3 AmbientDown;

float3 DirToLight;
float3 DirLightColor;

float3 PointLightPos;
float PointLightRangeReciprocal;
float3 PointLightColor;
float PointLightIntensity;


//////////////////////////////////////////////////////////////////////

struct Point_PixelShaderInput
{
    float4 Position : SV_POSITION;
    float4 ScreenPosition : TEXCOORD0;
};

Point_PixelShaderInput vs_PointLight(Point_VertexShaderInput input)
{
    Point_PixelShaderInput output;
    
    float4 pos = mul(float4(input.Position, 1), WorldViewProjection);
    
    output.Position = pos;
    output.ScreenPosition = pos;

    return output;
}

//////////////////////////////////////////////////////////////////////
////  Pixel Shaders
//////////////////////////////////////////////////////////////////////

float3 calcAmbient(Material material)
{
    // Convert from [-1, 1] to [0, 1];
    float up = material.normal.z * 0.5 + 0.5;
    float3 ambient = AmbientDown + up * AmbientUpRange;
    float emissive = material.emissive;
    
    return ambient * material.diffuseColor.rgb 
         + emissive * material.diffuseColor.rgb;
}

float3 calcDirectional(float3 position, Material material)
{
    // Phong diffuse
    float NDotL = dot(DirToLight, material.normal);
    float3 finalColor = DirLightColor.rgb * saturate(NDotL);

    // Blinn specular
    float3 toEye = EyePosition - position;
    toEye = normalize(toEye);
    float3 halfWay = normalize(toEye + DirToLight);
    float NDotH = saturate(dot(halfWay, material.normal));
    finalColor += DirLightColor.rgb * pow(NDotH, material.specExp) * material.specIntensity;
    
    return finalColor * material.diffuseColor.rgb;
}

float3 calcPointLight(float3 position, Material material)
{
    float3 toLight = PointLightPos.xyz - position;
    float3 toEye = EyePosition - position;
    
    // Phong diffuse
    float distToLight = length(toLight);
    toLight /= distToLight;
    float NDotL = saturate(dot(toLight, material.normal));
    float3 finalColor = PointLightColor.rgb * NDotL;
    
    // Blinn specular
    toEye = normalize(toEye);
    float3 halfWay = normalize(toEye + toLight);
    float NDotH = saturate(dot(halfWay, material.normal));
    finalColor += PointLightColor.rgb * pow(NDotH, material.specExp) * material.specIntensity;

    // Attenuation
    float distToLightNorm = 1 - saturate(distToLight * PointLightRangeReciprocal);
    float attenuation = distToLightNorm * distToLightNorm;
    finalColor *= material.diffuseColor.rgb * attenuation;
    finalColor *= PointLightIntensity;
    
    return finalColor;
}

float4 ps_Ambient(FullScreen_PixelShaderInput input) : COLOR
{
    Surface surface = unpackGBuffer(input.TexCoords);
    Material mat = createMaterial(surface);
    
    // Calculate amgbient and directional light contributions
    float4 finalColor;
    finalColor.rgb = calcAmbient(mat);
    finalColor.a = 1;
    
    return finalColor;
}

float4 ps_Direction(FullScreen_PixelShaderInput input) : COLOR
{
    Surface surface = unpackGBuffer(input.TexCoords);
    Material mat = createMaterial(surface);
    float3 position = calcWorldPos(input.TexCoords, surface.depth);
    
    // Calculate amgbient and directional light contributions
    float4 finalColor;
    finalColor.rgb = calcDirectional(position, mat);
    finalColor.a = 1; 
    
    return finalColor;
}

float4 ps_PointLight(Point_PixelShaderInput input) : COLOR
{
    float2 screenPos = input.ScreenPosition.xy / input.ScreenPosition.ww;
    float2 texCoord = 0.5 * (float2(screenPos.x, -screenPos.y) + 1);
    
    Surface surface = unpackGBuffer(texCoord);
    Material mat = createMaterial(surface);
    float3 position = calcWorldPos(texCoord, surface.depth);
    
    float4 finalColor;
    finalColor.rgb = calcPointLight(position, mat);
    finalColor.a = 1.0;
    
    return finalColor;
}

//////////////////////////////////////////////////////////////////////
////  Techniques
//////////////////////////////////////////////////////////////////////


technique AmbientAndEmissiveLighting
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_Ambient();
    }
}

technique DirectionalLighting
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_FullScreen();
        PixelShader = compile PSMODEL ps_Direction();
    }
}

technique PointLight
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_PointLight();
        PixelShader = compile PSMODEL ps_PointLight();
    }
}
