#version 330 core

layout (location = 0) in vec2 inPos;
layout (location = 2) in vec2 inTexCoords;

out vec2 texCoords;

void main()
{
    gl_Position = vec4(inPos.x, inPos.y, 0.0, 1.0);
    texCoords = inTexCoords;
}
    #FRAGMENT
    #version  330 core

out vec4 FragColor;

in vec2 texCoords;

#define NUM_SAMPLES 8
#define NUM_NOISE   4

uniform mat4 projection;

uniform vec3 samples[NUM_SAMPLES];
uniform vec3 noise[NUM_NOISE];

uniform sampler2D screenTexture;
uniform sampler2D screenTexturePos;
uniform sampler2D screenTextureNormal;



void main()
{


    const float gamma = 2.2;
    const float exposure = 1;
    
    //color = texture( screenTexture, texCoords.xy);

    int   size       = 5;
    float separation = 2;
    float threshold  = 0.9;
    float amount     = 0.4;

    vec2 texSize = textureSize(screenTexture, 0).xy;

    vec4 result = vec4(0.0);
    vec4 color  = vec4(0.0);

    float value = 0.0;
    float count = 0.0;

    for (int i = -size; i <= size; ++i) {
        for (int j = -size; j <= size; ++j) {
            color =
            texture
            ( screenTexture
            ,   (vec2(i, j) * separation + gl_FragCoord.xy)
            / texSize
            );

            // exposure tone mapping

            value = max(color.r, max(color.g, color.b));
            if (value < threshold) { color = vec4(0.0); }

            result += color;
            count  += 1.0;
        }
    }

    result /= count;
    
    vec3 hdrColor = texture(screenTexture, texCoords).rgb;

    // exposure tone mapping
    vec3 mapped = vec3(1.0) - exp(-hdrColor * exposure);
    // gamma correction 
    mapped = pow(mapped, vec3(1.0 / gamma));

    FragColor = vec4(mapped + vec3(result), 1.0);
    
}