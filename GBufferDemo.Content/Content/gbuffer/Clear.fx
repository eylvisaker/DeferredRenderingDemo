#if HLSL
#define VSMODEL vs_5_0
#define PSMODEL ps_5_0
#else
#define VSMODEL vs_3_0
#define PSMODEL ps_3_0
#endif

////////////////////////////////
// Vertex Inputs
////////////////////////////////
struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct PSIN
{
    float4 Position : SV_POSITION;
};

struct PSOUT_GBuffer
{
    float4 Color : COLOR0;
    float4 Depth : COLOR1;
    float4 Normal : COLOR2;
    float4 Spec : COLOR3;
};

//////////////////////////////////////////////////////////////////////
////  Standard Vertex Shader
//////////////////////////////////////////////////////////////////////

PSIN vs_Clear(VertexShaderInput input)
{
    PSIN output;

    output.Position = input.Position;
    
    return output;
}

//////////////////////////////////////////////////////////////////////
////  Pixel Shaders
//////////////////////////////////////////////////////////////////////

PSOUT_GBuffer ps_Clear(PSIN input)
{
    PSOUT_GBuffer result;
    
    result.Color = float4(0, 0, 0, 0);
    result.Depth = 0;
    result.Normal = float4(0, 0, 0, 0);
    result.Spec = float4(0, 0, 0, 0);
    
    return result;
}

////////////////////////////////////////////////////////////////////////////
////  Techniques
////////////////////////////////////////////////////////////////////////////

technique Clear
{
    pass Pass0
    {
        VertexShader = compile VSMODEL vs_Clear();
        PixelShader = compile PSMODEL ps_Clear();
    }
}
