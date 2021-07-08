
struct VertexIn
{
    float2 Position : POSITION0;
    float2 TexCoords : TEXCOORD0;
};

struct FragmentIn
{
    float4 Position : SV_Position;
    float2 TexCoords : TEXCOORD0;
};

FragmentIn main(VertexIn input)
{
    FragmentIn output;
    output.TexCoords = input.TexCoords;
    output.Position = float4(input.Position.x, input.Position.y, 0, 1);
    return output;
}
