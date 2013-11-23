float4x4 World;
float4x4 View;
float4x4 Projection;

// TODO: add effect parameters here.

struct VertexShaderInput
{
    float4 Position : POSITION0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

// Our texture sampler
texture Texture;
sampler TextureSampler = sampler_state
{
    Texture = <Texture>;
};
 
// This data comes from the SpriteBatch Vertex Shader
struct VertexShaderOutput
{
    float4 Position : TEXCOORD0;
    float4 Color : COLOR0;
    float2 TextureCordinate : TEXCOORD0;
};
 
// Our Pixel Shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = tex2D(TextureSampler, input.TextureCordinate);
    return color;
}

// Compile our shader
technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
