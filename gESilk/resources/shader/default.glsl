#version 430

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec3 vNormal;
layout(location = 2) in vec2 vTexCoord;
layout(location = 3) in vec3 vTangent;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;
uniform mat4 lightView;
uniform mat4 lightProjection;

out vec3 FragPos;
out vec2 fTexCoord;
out mat3 TBN;
out vec4 FragPosLightSpace;
out vec3 viewFragPos;
out vec3 viewNormal;
out vec3 noNormalNormal;


void main() {
    fTexCoord = vTexCoord;
    mat3 normalMatrix =  mat3(transpose(inverse(model)));
    vec3 T = normalize(vTangent * normalMatrix);
    vec3 N = normalize(vNormal * normalMatrix);
    vec3 B = cross(N, T);
    TBN = transpose(mat3(T, B, N));
    vec4 worldPos = vec4(vPosition, 1.0) * model;
    FragPos = vec3(worldPos);
    gl_Position = worldPos * view * projection;
    FragPosLightSpace = worldPos * lightView * lightProjection;
    viewFragPos = vec3(worldPos * view);
    viewNormal = normalize(vNormal * mat3(transpose(inverse(model*view))));
    noNormalNormal = N;
}
    //-FRAGMENT-
    #version 430

uniform samplerCube skyBox;
uniform sampler2D albedo;
uniform sampler2D normalMap;
uniform sampler2DShadow shadowMap;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform float emission = 1.0;
uniform sampler2D specularTex;
uniform float ior = 1.450;
uniform float normalStrength = 1.0;
uniform float worldStrength = 1.0;
uniform vec3 cubemapLoc = vec3(0);
uniform vec3 cubemapScale = vec3(1);
uniform samplerCube skyboxGlobal;

in vec3 FragPos;
in vec2 fTexCoord;
in mat3 TBN;
in vec4 FragPosLightSpace;
in vec3 viewFragPos;
in vec3 viewNormal;
in vec3 noNormalNormal;

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 FragNormal;
layout (location = 2) out vec4 FragLoc;

const int pcfCount = 4;
const float totalTexels = pow(pcfCount * 2.0 + 1.0, 2);

bool isOutOfBounds(vec3 box, vec3 position){
    if (box.x > position.x || box.y > position.y || box.z > position.z) return true; else return false;
}


vec3 CubemapParallaxUV(vec3 normal) {
    vec3 dirWS = FragPos - viewPos;
    vec3 reflectionDirWS = reflect(dirWS, normal);
    
    vec3 BoxMax = cubemapScale + cubemapLoc;
    vec3 BoxMin = cubemapScale * -1 + cubemapLoc;  
    
    vec3 FirstIntersect = (BoxMax - FragPos) / reflectionDirWS;
    vec3 SecondIntersect = (BoxMin - FragPos) / reflectionDirWS;
    
    vec3 Furthest = max(FirstIntersect, SecondIntersect);
    float Distance = min(min(Furthest.x, Furthest.y), Furthest.z);
    
    vec3 IntersectPos = FragPos + reflectionDirWS * Distance;
    
    if (isOutOfBounds(BoxMin, FragPos) || !isOutOfBounds(BoxMax, FragPos))
    {
        return reflectionDirWS;
    }
    
    return IntersectPos - cubemapLoc;
}

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    float currentDepth = projCoords.z;
    float bias = max(0.001 * (1.0 - dot(normal, lightDir)), 0.001);
    float visibility = 0;

    float texelSize = 1.0/textureSize(shadowMap, 0).x;
    for (int x = -pcfCount; x<= pcfCount; x++){
        for (int y =-pcfCount; y<=pcfCount; y++){
            visibility += texture(shadowMap, vec3(projCoords.xy+vec2(x, y)*texelSize, currentDepth - bias));
        }
    }
    visibility /= totalTexels;
    if (projCoords.z > 1.0) visibility = 1;

    return visibility;
}

float fresnelSchlickRoughness(float cosTheta, float F0, float roughness)
{
    return F0 + (max(1.0 - roughness, F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

vec3 fresnelSchlickRoughnessColor(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}   

void main()
{	

    vec3 albedoSample = texture(albedo, fTexCoord).rgb;

    vec3 normal = texture(normalMap, fTexCoord).rgb * vec3(1, -1, 1) + vec3(0, 1, 0);
    normal = normalize((normal * 2.0 - 1.0) * TBN);
    normal = mix(normalize(noNormalNormal), normal, normalStrength);

    vec3 lightDir = normalize(lightPos);
    vec3 viewDir = normalize(viewPos - FragPos); 
    
    vec3 specular = texture(specularTex, fTexCoord).rgb;
    float ao = specular.r;
    float roughness = specular.g;
    float metallic = specular.b;

    float mipmapLevel = float(textureQueryLevels(skyBox));
   
    vec3 skyboxIrradiance = textureLod(skyBox, normal, mipmapLevel * 0.7).rgb;
   
    float ambient = max(0, dot(lightDir, normal))*0.5+0.5;
    ambient = min(ambient, min(1, ShadowCalculation(FragPosLightSpace, noNormalNormal, lightDir)+0.5));
   
    
    vec4 skyboxWithAlpha = textureLod(skyBox, CubemapParallaxUV(normal), roughness * mipmapLevel);
    vec3 skyboxSampler = skyboxWithAlpha.rgb;
    if (skyboxWithAlpha.a < 0.5) {
        skyboxSampler = textureLod(skyboxGlobal, reflect(-viewDir, normal), roughness * mipmapLevel).rgb;
    } 
    
    //skyboxSampler = vec3(1.0) - exp(-skyboxSampler); 
    
    ambient = mix(ambient, 1.0, metallic);
    albedoSample *= (skyboxIrradiance * 0.2) + ambient;
    
    vec3 halfwayDir = normalize(lightDir + viewDir);  
    float spec = pow(max(dot(normal, halfwayDir), 0.0), pow(2-roughness, 8)) * (1-roughness);
    
    albedoSample = mix(albedoSample + (skyboxSampler * fresnelSchlickRoughness(dot(viewDir, normal), 0.04, roughness)) + spec, albedoSample * (skyboxSampler + spec), metallic);
    
    FragColor = vec4(albedoSample, 1.0);
    FragLoc = vec4(viewFragPos, metallic);
    FragNormal = vec4(viewNormal, dot(normalize(noNormalNormal), viewDir));
}