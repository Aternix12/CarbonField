#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D grassTexture : register(s0);
Texture2D dirtTexture : register(s1);
sampler2D grassSampler = sampler_state { Texture = <grassTexture>; };
sampler2D dirtSampler = sampler_state { Texture = <dirtTexture>; };

// Vertex shader output structure
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

// Pixel shader
float4 BlendPS(VertexShaderOutput input) : COLOR
{
    float4 grassColor = tex2D(grassSampler, input.TextureCoordinates);
    float4 dirtColor = tex2D(dirtSampler, input.TextureCoordinates);
    float blendFactor = 0.5; // Example blend factor
    return lerp(dirtColor, grassColor, blendFactor) * input.Color;
}

// Technique definition
technique BlendDrawing
{
    pass P0
    {
        // MonoGame's content pipeline will handle the conversion for OpenGL
        PixelShader = compile PS_SHADERMODEL BlendPS();
    }
};
