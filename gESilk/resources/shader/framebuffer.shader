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
    float radius    = 0.6;
    float bias      = 0.005;
    float magnitude = 1.1;
    float contrast  = 1.1;

    vec2 texSize  = textureSize(screenTexturePos, 0).xy;
    vec2 texCoord = gl_FragCoord.xy / texSize;

    vec4 position = texture(screenTexturePos, texCoord);
    if (position.a <= 0) { return; }

    vec3 normal = normalize(texture(screenTextureNormal, texCoord).xyz);

    int  noiseS = int(sqrt(NUM_NOISE));
    int  noiseX = int(gl_FragCoord.x - 0.5) % noiseS;
    int  noiseY = int(gl_FragCoord.y - 0.5) % noiseS;
    vec3 random = noise[noiseX + (noiseY * noiseS)];

    vec3 tangent  = normalize(random - normal * dot(random, normal));
    vec3 binormal = cross(normal, tangent);
    mat3 tbn      = mat3(tangent, binormal, normal);

    float occlusion = NUM_SAMPLES;

    for (int i = 0; i < NUM_SAMPLES; ++i) {
        vec3 samplePosition = tbn * samples[i];
        samplePosition = position.xyz + samplePosition * radius;

        vec4 offsetUV      = vec4(samplePosition, 1.0);
        offsetUV      = projection * offsetUV;
        offsetUV.xyz /= offsetUV.w;
        offsetUV.xy   = offsetUV.xy * 0.5 + 0.5;

        // Config.prc
        // gl-coordinate-system  default
        // textures-auto-power-2 1
        // textures-power-2      down

        vec4 offsetPosition = texture(screenTextureNormal, offsetUV.xy);

        float occluded = 0;
        if   (samplePosition.y + bias <= offsetPosition.y)
        { occluded = 0; }
        else { occluded = 1; }

        float intensity =
        smoothstep
        ( 0
        , 1
        ,   radius
        / abs(position.y - offsetPosition.y)
        );
        occluded  *= intensity;
        occlusion -= occluded;
    }

    occlusion /= NUM_SAMPLES;
    occlusion  = pow(occlusion, magnitude);
    occlusion  = contrast * (occlusion - 0.5) + 0.5;

    FragColor = pow(texture(screenTexture, texCoords), vec4(1/2.2));
}