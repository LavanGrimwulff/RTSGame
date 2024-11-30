
// Declare matrices and light direction
float4x4 World;
float4x4 View;
float4x4 Projection;
float4 LightDir; 


float4 LightColor;
float4 Ambient;
float4 CameraPosition;


float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 99;
float4 ambientIntensity;
float fish = 0;

sampler s0;
texture lightMask;
sampler2D lightSampler = sampler_state
{
    Texture = (lightMask);
};
// texture voodoo 
texture ModelTexture;
sampler2D textureSampler = sampler_state {
    Texture = (ModelTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = wrap;
    AddressV = wrap;
};
float distance3d(float4 input) // Finds 3d hypotenuse
{
    return sqrt(input[0] * input[0] + input[1] * input[1] + input[2] * input[2]);
};

// each input vertex contains a position, normal and texture
struct VertexShaderInput
{

    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0; 
    
};

// the values to be interpolated across triangle
struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TextureCoordinate : TEXCOORD0; 
    float4 Normal : TEXCOORD1;

 //   float4 worldPosition : TEXCOORD1;
    
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    if(fish == 1)
    {
        float4 worldPosition = mul(input.Position, World);
        float4 viewPosition = mul(worldPosition, View);
        float z = viewPosition[2];
        float square = distance3d(viewPosition) * (z < 0 ? -1 : 1);
        viewPosition[2] = square;
        output.Position = mul(viewPosition, Projection);
    }
    else
    {
        float4 worldPosition = mul(input.Position, World); 
        float4 viewPosition = mul(worldPosition, View); 
    
        output.Position = mul(viewPosition, Projection);

    }
 
    
	output.TextureCoordinate = input.TextureCoordinate;
	output.Normal = input.Normal;
    
    
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);  // get texture color

    textureColor *= dot(LightDir, input.Normal);
    
    textureColor += (ambientIntensity * AmbientColor);
    
    //FoW
    float4 color = tex2D(s0, input.TextureCoordinate);
    float4 lightColor = tex2D(lightSampler, input.TextureCoordinate);
    
	textureColor *= lightColor;
	
    return saturate(textureColor);

}

technique Technique1
{
    pass Pass1
    {
		VertexShader = compile vs_3_0  VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
