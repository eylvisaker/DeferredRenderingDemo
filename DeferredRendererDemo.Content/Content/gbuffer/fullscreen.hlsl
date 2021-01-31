

float2 TexelOffset;

//////////////////////////////////////////////////////////////////////

struct FullScreen_VertexShaderInput
{
    float4 Position : POSITION;
};

struct FullScreen_PixelShaderInput
{
    float4 Position : SV_POSITION;
    float2 TexCoords : TEXCOORD0;
};



//////////////////////////////////////////////////////////////////////
////  Standard Vertex Shader
//////////////////////////////////////////////////////////////////////

FullScreen_PixelShaderInput vs_FullScreen(FullScreen_VertexShaderInput input)
{
    FullScreen_PixelShaderInput output;
    
    output.Position = input.Position;
    output.TexCoords.x = (1 + input.Position.x) / 2;
    output.TexCoords.y = (1 - input.Position.y) / 2;

    output.TexCoords += TexelOffset;
    
    return output;
}

struct Point_VertexShaderInput
{
    float3 Position : POSITION;
};