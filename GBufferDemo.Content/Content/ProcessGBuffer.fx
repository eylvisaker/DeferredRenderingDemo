#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_3_0
#define PSMODEL ps_3_0
#endif

texture ColorTexture;
texture DepthTexture;
texture NormalTexture;

float4x4 WorldViewProjection;

float4 PerspectiveValues;
float4x4 ViewProjectionInv;
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

struct Direction_VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
};

struct Point_VertexShaderInput
{
    float3 Position : POSITION;
};

//////////////////////////////////////////////////////////////////////

struct Direction_PixelShaderInput
{
    float4 Position : SV_POSITION;
    float2 TexCoords : TEXCOORD0;
};

struct Point_PixelShaderInput
{
    float4 Position : SV_POSITION;
    float4 ScreenPosition : TexCoords : TEXCOORD0;
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

//////////////////////////////////////////////////////////////////////
////  Reading the GBuffer
//////////////////////////////////////////////////////////////////////

static const float2 g_SpecExpRange = { 0.1, 250.0 };

struct Surface
{
    float LinearDepth;
    float Depth;
    float3 Color;
    float3 Normal;
    float SpecInt;
    float SpecPow;
};

float ConvertDepthToLinear(float depth)
{
    float linearDepth = PerspectiveValues.z / (depth + PerspectiveValues.w);
    return linearDepth;
}

Surface UnpackGBuffer(float2 texCoords)
{
    Surface result;
    
    float4 color = tex2D(ColorSampler, texCoords);
    float depth = tex2D(DepthSampler, texCoords).x;
    float3 normal = tex2D(NormalSampler, texCoords).xyz;
    
    result.Color = color.xyz;
    result.Depth = depth;
    result.LinearDepth = ConvertDepthToLinear(depth);
    result.Normal = normal * 2 - 1;
    result.SpecInt = 0;
    result.SpecPow = 0;
    
    return result;
}

//////////////////////////////////////////////////////////////////////
////  Standard Vertex Shader
//////////////////////////////////////////////////////////////////////

Direction_PixelShaderInput vs_Direction(Direction_VertexShaderInput input)
{
    Direction_PixelShaderInput output;
    
    output.Position = input.Position;
    output.TexCoords = input.TexCoords;

    return output;
}

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

struct Material
{
    float3 normal;
    float4 diffuseColor;
    float specExp;
    float specIntensity;
};

Material CreateMaterial(Surface surface)
{
    Material mat;
    
    mat.normal = surface.Normal;
    mat.diffuseColor = float4(surface.Color.xyz, 1);
    mat.specExp = g_SpecExpRange.x + g_SpecExpRange.y * surface.SpecPow;
    mat.specIntensity = surface.SpecInt;
   
    return mat;
}

float3 CalcWorldPos(float2 csPos, float depth)
{
    float4 position;
    
    position.xy = csPos.xy;
    position.z = depth;
    position.w = 1.0;
    
    float4 result = mul(position, ViewProjectionInv);
    result /= result.w;

    return result;
}

float3 CalcAmbient(float3 normal, float3 color)
{
    // Convert from [-1, 1] to [0, 1];
    float up = normal.z * 0.5 + 0.5;
    float3 ambient = AmbientDown + up * AmbientUpRange;
    
    return ambient * color;
}

float3 CalcDirectional(float3 position, Material material)
{
    // Phong diffuse
    float NDotL = dot(DirToLight, material.normal);
    float3 finalColor = DirLightColor.rgb * saturate(NDotL);

    // Blinn specular
    float3 toEye = EyePosition - position;
    toEye = normalize(toEye);
    float3 halfWay = normalize(toEye + DirToLight);
    float3 NDotH = saturate(dot(halfWay, material.normal));
    finalColor += DirLightColor.rgb * pow(NDotH, material.specExp);

    return finalColor * material.diffuseColor.rgb;
}

float3 CalcPointLight(float3 position, Material material)
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

float4 ps_Direction(Direction_PixelShaderInput input) : COLOR
{
    Surface surface = UnpackGBuffer(input.TexCoords);
    Material mat = CreateMaterial(surface);
    float3 position = CalcWorldPos(input.TexCoords, surface.LinearDepth);
    
    // Calculate amgbient and directional light contributions
    float4 finalColor;
    finalColor.rgb = CalcAmbient(mat.normal, mat.diffuseColor.rgb);
    finalColor.rgb += CalcDirectional(position, mat);
    finalColor.a = 1;
    
    return finalColor;
}


float4 ps_PointLight(Point_PixelShaderInput input) : COLOR
{
    float2 screenPos = input.ScreenPosition.xy / input.ScreenPosition.ww;
    float2 texCoord = 0.5 * (float2(screenPos.x, -screenPos.y) + 1);
    
    Surface surface = UnpackGBuffer(texCoord);
    Material mat = CreateMaterial(surface);
    float3 position = CalcWorldPos(screenPos, surface.Depth);
    
    float4 finalColor;
    finalColor.xyz = CalcPointLight(position, mat);
    finalColor.w = 1.0;
    
    return finalColor;
}
//////////////////////////////////////////////////////////////////////
////  Techniques
//////////////////////////////////////////////////////////////////////


technique DirectionalLighting
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_Direction();
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

