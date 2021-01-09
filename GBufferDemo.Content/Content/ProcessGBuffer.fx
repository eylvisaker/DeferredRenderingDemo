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

float4 PerspectiveValues;
float4x4 ViewInv;

float4 AmbientUpRange;
float4 AmbientDown;

float3 DirToLight;
float3 DirLightColor;

//////////////////////////////////////////////////////////////////////

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
};


//////////////////////////////////////////////////////////////////////

struct PixelShaderInput
{
    float4 Position : SV_POSITION;
    float2 TexCoords : TEXCOORD0;
};

sampler ColorSampler = sampler_state
{
    Texture = <ColorTexture>;
};

sampler DepthSampler = sampler_state
{
    Texture = <DepthTexture>;
};

sampler NormalSampler = sampler_state
{
    Texture = <NormalTexture>;
};

//////////////////////////////////////////////////////////////////////
////  Reading the GBuffer
//////////////////////////////////////////////////////////////////////

const float2 g_SpecExpRange = { 0.1, 250.0 };

struct Surface
{
    float LinearDepth;
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
    result.LinearDepth = ConvertDepthToLinear(depth);
    result.Normal = normal * 2 - 1;
    result.SpecInt = 1;
    result.SpecPow = 5;
    
    return result;
}

//////////////////////////////////////////////////////////////////////
////  Standard Vertex Shader
//////////////////////////////////////////////////////////////////////

PixelShaderInput vs_Direction(VertexShaderInput input)
{
    PixelShaderInput output;
    
    output.Position = input.Position;
    output.TexCoords = input.TexCoords;

    return output;
}

//////////////////////////////////////////////////////////////////////
////  Pixel Shaders
//////////////////////////////////////////////////////////////////////

#define EyePosition ViewInv[3].xyz;

struct Material
{
    float3 normal;
    float4 diffuseColor;
    float specExp;
    float specIntensity;
};

float3 CalcWorldPos(float2 csPos, float linearDepth)
{
    float4 position;
    
    position.xy = csPos.xy * PerspectiveValues.xy * linearDepth;
    position.z = linearDepth;
    position.w = 1.0;
    
    return mul(position, ViewInv).xyz;
}

float3 CalcAmbient(float3 normal, float3 color)
{
    // Convert from [-1, 1] to [0, 1];
    float up = normal.z * 0.5 + 0.5;
    float ambient = AmbientDown + up * AmbientUpRange;
    
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

float4 ps_Direction(PixelShaderInput input) : COLOR
{
    Surface surface = UnpackGBuffer(input.TexCoords);
    
    Material mat;
    mat.normal = surface.Normal;
    mat.diffuseColor = float4(surface.Color.xyz, 1);
    mat.specExp = g_SpecExpRange.x + g_SpecExpRange.y * surface.SpecPow;
    mat.specIntensity = surface.SpecInt;
    
    // Reconstruct the world position
    float3 position = CalcWorldPos(input.TexCoords, surface.LinearDepth);
    
    // Calculate amgbient and directional light contributions
    float4 finalColor;
    finalColor.xyz = CalcAmbient(mat.normal, mat.diffuseColor.xyz);
    finalColor.xyz += CalcDirectional(position, mat);
    finalColor.w = 1;
    
    return finalColor;
}

technique DirectionalLighting
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_Direction();
        PixelShader = compile PSMODEL ps_Direction();
    }
}

