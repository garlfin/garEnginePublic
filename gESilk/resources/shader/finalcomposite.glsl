#version 330 core

layout (location = 0) in vec2 inPos;
layout (location = 2) in vec2 inTexCoords;

out vec2 TexCoord;

void main()
{
    gl_Position = vec4(inPos.x, inPos.y, 0.0, 1.0);
    TexCoord = inTexCoords;
}

-FRAGMENT-

#version  330 core

out vec4 FragColor;

in vec2 TexCoord;

const float gamma = 2.2;
const float exposure = 0.8;

uniform sampler2D screenTexture;
uniform sampler2D ao;
uniform sampler2D bloom;
uniform float bloomIntensity = 0.8;

const vec2 u_texelStep = vec2(1.0/1280, 1.0/720);

const float u_mulReduce = 9.0;
const float u_minReduce = 128.0;
const float u_maxSpan = 8.0;

void main()
{

    // From McNoppers OpenGL repository

    vec3 ScreenTex = texture(screenTexture, TexCoord).rgb;

    vec3 outScreenTex = vec3(1.0);

    vec3 rgbNW = textureOffset(screenTexture, TexCoord, ivec2(-1, 1)).rgb;
    vec3 rgbNE = textureOffset(screenTexture, TexCoord, ivec2(1, 1)).rgb;
    vec3 rgbSW = textureOffset(screenTexture, TexCoord, ivec2(-1, -1)).rgb;
    vec3 rgbSE = textureOffset(screenTexture, TexCoord, ivec2(1, -1)).rgb;

    // see http://en.wikipedia.org/wiki/Grayscale
    const vec3 toLuma = vec3(0.299, 0.587, 0.114);

    float lumaNW = dot(rgbNW, toLuma);
    float lumaNE = dot(rgbNE, toLuma);
    float lumaSW = dot(rgbSW, toLuma);
    float lumaSE = dot(rgbSE, toLuma);
    float lumaM = dot(ScreenTex, toLuma);

    // Gather minimum and maximum luma.
    float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
    float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));

    // Sampling is done along the gradient.
    vec2 samplingDirection;
    samplingDirection.x = -((lumaNW + lumaNE) - (lumaSW + lumaSE));
    samplingDirection.y =  ((lumaNW + lumaSW) - (lumaNE + lumaSE));

    // Sampling step distance depends on the luma: The brighter the sampled texels, the smaller the final sampling step direction.
    // This results, that brighter areas are less blurred/more sharper than dark areas.  
    float samplingDirectionReduce = max((lumaNW + lumaNE + lumaSW + lumaSE) * 0.25 * u_mulReduce, u_minReduce);

    // Factor for norming the sampling direction plus adding the brightness influence. 
    float minSamplingDirectionFactor = 1.0 / (min(abs(samplingDirection.x), abs(samplingDirection.y)) + samplingDirectionReduce);

    // Calculate final sampling direction vector by reducing, clamping to a range and finally adapting to the texture size. 
    samplingDirection = clamp(samplingDirection * minSamplingDirectionFactor, vec2(-u_maxSpan), vec2(u_maxSpan)) * u_texelStep;

    vec3 rgbSampleNeg = texture(screenTexture, TexCoord + samplingDirection * (1.0/3.0 - 0.5)).rgb;
    vec3 rgbSamplePos = texture(screenTexture, TexCoord + samplingDirection * (2.0/3.0 - 0.5)).rgb;

    vec3 rgbTwoTab = (rgbSamplePos + rgbSampleNeg) * 0.5;

    vec3 rgbSampleNegOuter = texture(screenTexture, TexCoord + samplingDirection * (0.0/3.0 - 0.5)).rgb;
    vec3 rgbSamplePosOuter = texture(screenTexture, TexCoord + samplingDirection * (3.0/3.0 - 0.5)).rgb;

    vec3 rgbFourTab = (rgbSamplePosOuter + rgbSampleNegOuter) * 0.25 + rgbTwoTab * 0.5;

    float lumaFourTab = dot(rgbFourTab, toLuma);

    if (lumaFourTab < lumaMin || lumaFourTab > lumaMax)
    {
        outScreenTex = rgbTwoTab;
    }
    else
    {
        outScreenTex = rgbFourTab;
    }

    float aoData = texture(ao, TexCoord).r;
    vec3 hdrColor = outScreenTex * aoData + (texture(bloom, TexCoord).rgb * bloomIntensity);

    // exposure tone mapping
    vec3 mapped = vec3(1.0) - exp(-hdrColor * exposure);
    // gamma correction 
    mapped = pow(mapped, vec3(1.0 / gamma));
    FragColor = vec4(mapped, 1.0);
}