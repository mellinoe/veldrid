
struct FragmentIn
{
    float4 Position : SV_Position;
    float2 TexCoords : TEXCOORD0;
};

[[vk::binding(0)]]
Texture2D SourceTexture;
[[vk::binding(1)]]
SamplerState SourceSampler;

const bool OutputFormatSrgb = true;

float3 LinearToSrgb(float3 linearColor)
{
    // http://chilliant.blogspot.com/2012/08/srgb-approximations-for-hlsl.html
    float3 S1 = sqrt(linearColor);
    float3 S2 = sqrt(S1);
    float3 S3 = sqrt(S2);
    float3 sRGB = 0.662002687 * S1 + 0.684122060 * S2 - 0.323583601 * S3 - 0.0225411470 * linearColor;
    return sRGB;
}

float4 main(FragmentIn input) : SV_Target0
{
    float4 color = SourceTexture.Sample(SourceSampler, input.TexCoords);

    if (!OutputFormatSrgb)
    {
        color = float4(LinearToSrgb(color.rgb), 1);
    }

    return color;
}
