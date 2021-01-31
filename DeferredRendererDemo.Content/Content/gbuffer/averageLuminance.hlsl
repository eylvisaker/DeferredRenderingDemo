
Texture2D AverageLuminanceTexture;


sampler AverageLuminanceSampler = sampler_state
{
    Texture = <AverageLuminanceTexture>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = linear;
    MaxAnisotropy = 1;
    AddressU = CLAMP;
    AddressV = CLAMP;
};


static const float4 LUM_FACTOR = float4(0.299, 0.587, 0.114, 0);


float avgLuminance(float2 texCoords)
{
    float avgLum = dot(AverageLuminanceTexture.Sample(AverageLuminanceSampler, texCoords), LUM_FACTOR);

    avgLum += 0.001;
    
    return avgLum;
}