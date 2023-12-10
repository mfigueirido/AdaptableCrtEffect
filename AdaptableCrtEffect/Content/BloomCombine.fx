// Pixel shader combines the bloom image with the original
// scene, using tweakable intensity levels and saturation.
// This is the final step in applying a bloom postprocess.

sampler BloomSampler : register(s0)
{
    Filter = LINEAR;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler BaseSampler : register(s1)
{
    Texture = (BaseTexture);
    Filter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float BloomIntensity = 1;
float BloomSaturation = 1;

// Helper for modifying the saturation of a color.
float3 AdjustSaturation(float3 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, float3(0.3, 0.59, 0.11));

    return lerp(grey, color, saturation);
}

float4 BloomCombineFunction(float4 pos : SV_POSITION, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the bloom and original base image colors.
    float3 bloom = tex2D(BloomSampler, texCoord).rgb;
    float3 base = tex2D(BaseSampler, texCoord).rgb;

    // Adjust color saturation and intensity and combine both images
    base += max(0, AdjustSaturation(bloom, BloomSaturation)) * BloomIntensity;
    
    return float4(base, 1.0f);
}

float4 BloomSaturateFunction(float4 pos : SV_POSITION, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the bloom and original base image colors.
    float3 bloom = tex2D(BloomSampler, texCoord).rgb;

    // Adjust color saturation and intensity and combine both images.
    // Multiply by vertex color so the effect can be turned off from outside.
    bloom = max(0, AdjustSaturation(bloom, BloomSaturation)) * BloomIntensity * color.rgb;
    
    return float4(bloom, 1.0f);
}

technique BloomCombine
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 BloomCombineFunction();
    }
}

technique BloomSaturate
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 BloomSaturateFunction();
    }
}