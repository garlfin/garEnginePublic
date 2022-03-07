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

out float FragColor;

in vec2 TexCoord;

uniform sampler2D screenTexturePos;
uniform sampler2D screenTextureNormal;
uniform sampler2D NoiseTex;
uniform vec3 Samples[64];
uniform mat4 projection;

int kernelSize = 64;
float SSAORadius  = 0.25;
float SSAOBias = 0.025;
// tile noise texture over screen based on screen dimensions divided by noise size
const vec2 NoiseScale = vec2(1280.0/4.0, 720.0/4.0);

void main()
{

    vec2 texSize = vec2(1280, 720);
    vec4 fragPosWA = texture(screenTexturePos, TexCoord);
    vec3 fragPos = fragPosWA.xyz;
    vec3 normal = texture(screenTextureNormal, TexCoord).rgb;
    vec3 randomVec = texture(NoiseTex, TexCoord * NoiseScale).xyz;
    vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
    vec3 bitangent = cross(normal, tangent);
    mat3 TBN = mat3(tangent, bitangent, normal);

    float occlusion = 0.0;



    for (int i = 0; i < 64; i++)
    {
        vec3 Sample= Samples[i];
        Sample= TBN * Sample;
        Sample= fragPos + Sample* SSAORadius;

        vec4 offset = vec4(Sample, 1.0);
        offset = offset * projection;
        offset.xyz /= offset.w;
        offset.xyz  = offset.xyz * 0.5 + 0.5;

        float sampleDepth = texture(screenTexturePos, offset.xy).z;

        float rangeCheck = smoothstep(0.0, 1.0, SSAORadius / abs(fragPos.z - sampleDepth));

        occlusion += (sampleDepth >= Sample.z + SSAOBias ? 1.0 : 0.0) * rangeCheck;
    }
    occlusion = 1.0 - (occlusion / kernelSize);

    occlusion = mix(occlusion, 1, fragPosWA.w);

    FragColor = occlusion;
}