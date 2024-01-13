#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D overlayTexture : register(s0);
sampler2D overlaySampler = sampler_state
{
    Texture = <overlayTexture>;
};
Texture2D blendMap : register(s1);
sampler2D blendMapSampler = sampler_state
{
    Texture = <blendMap>;
};

// Vertex shader output structure
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

// Pixel shader
float4 OverlayBlendPS(VertexShaderOutput input) : COLOR
{
    float4 overlayColor = tex2D(overlaySampler, input.TextureCoordinates);
    float blendFactor = tex2D(blendMapSampler, input.TextureCoordinates).r;

    return overlayColor * blendFactor * input.Color;
}


// Technique definition
technique BlendDrawing
{
    pass P0
    {
        // MonoGame's content pipeline will handle the conversion for OpenGL
        PixelShader = compile PS_SHADERMODEL OverlayBlendPS();
    }
};