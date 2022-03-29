// I love you Joey DeVries https://learnopengl.com/About -- BRDF Code
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
    
struct pointLight {
    vec3 Position;
    vec3 Color;
    float intensity;
    float radius;
};

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
uniform int lightsCount;
uniform int stage;
uniform sampler2D brdfLUT; 
uniform samplerCube irradianceTex;

#define MAX_LIGHTS 10

uniform pointLight lights[MAX_LIGHTS];
uniform samplerCube shadowMaps[MAX_LIGHTS];

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

const vec3 sampleOffsetDirections[20] = vec3[]
(
vec3( 1,  1,  1), vec3( 1, -1,  1), vec3(-1, -1,  1), vec3(-1,  1,  1),
vec3( 1,  1, -1), vec3( 1, -1, -1), vec3(-1, -1, -1), vec3(-1,  1, -1),
vec3( 1,  1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1,  1,  0),
vec3( 1,  0,  1), vec3(-1,  0,  1), vec3( 1,  0, -1), vec3(-1,  0, -1),
vec3( 0,  1,  1), vec3( 0, -1,  1), vec3( 0, -1, -1), vec3( 0,  1, -1)
);

bool isOutOfBounds(vec3 box, vec3 position){
    return box.x > position.x || box.y > position.y || box.z > position.z;
}

float ShadowCalculation(pointLight light, samplerCube depthMap, vec3 normal, vec3 viewPos)
{

    vec3 fragToLight = FragPos - light.Position;

    float currentDepth = length(fragToLight);

    float bias = max(0.05 * (1.0 - dot(normal, normalize(lightPos - FragPos))), 0.05);
    
    float diskRadius = (1.0 + (length(viewPos - FragPos) / 100)) / 25;
    
    float shadow = 0;
    
    for(int i = 0; i < 20; ++i)
    {
        float closestDepth = texture(depthMap, fragToLight + (sampleOffsetDirections[i] * diskRadius)).r;
        closestDepth *= 100;
        if(currentDepth - bias > closestDepth) shadow += 0.05;
    }
    return shadow;
}

const float PI = 3.14159265359;

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness*roughness;
    float a2 = a*a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;

    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}
// ----------------------------------------------------------------------------
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}


vec4 CubemapParallaxUV(vec3 normal) {
    vec3 reflectionDirWS = normal;
    
    vec3 BoxMax = cubemapScale + cubemapLoc;
    vec3 BoxMin = cubemapScale * -1 + cubemapLoc;  
    
    vec3 FirstIntersect = (BoxMax - FragPos) / reflectionDirWS;
    vec3 SecondIntersect = (BoxMin - FragPos) / reflectionDirWS;
    
    vec3 Furthest = max(FirstIntersect, SecondIntersect);
    float Distance = min(min(Furthest.x, Furthest.y), Furthest.z);
    
    vec3 IntersectPos = FragPos + reflectionDirWS * Distance;
    
    float outOfBounds = 0;
    
    if (isOutOfBounds(BoxMin, FragPos) || isOutOfBounds(FragPos, BoxMax)) outOfBounds = 1;
    
    
    return vec4(IntersectPos - cubemapLoc, outOfBounds);
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



vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}   

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

vec3 calculateLight(vec3 lightDir, vec3 viewDir, float roughness, vec3 F0, float metallic, vec3 normal, float shadow, vec3 albedoCopy, float radius){


    vec3 halfwayDir = normalize(lightDir + viewDir);
    radius = max(0.01, radius);
    roughness = clamp(roughness, radius * 0.1, 1.0);
    // cook-torrance brdf
    float NDF = DistributionGGX(normal, halfwayDir, roughness);
    float G   = GeometrySmith(normal, viewDir, lightDir, roughness);
    vec3 F    = fresnelSchlick(dot(halfwayDir, viewDir), F0);       
            
        vec3 kS = F;
        vec3 kD = vec3(1.0) - kS;
        kD *= 1.0 - metallic;	  
            
        vec3 numerator    = NDF * G * F;
        float denominator = 4.0 * max(dot(normal, viewDir), 0.0) * max(dot(normal, lightDir), 0.0) + 0.0001; // + 0.0001 to prevent divide by zero
        vec3 specular = numerator / denominator;
           
        float NdotL = max(dot(normal, lightDir), 0.0);    
       
        return (kD * albedoCopy / PI + specular) * NdotL * shadow;
   }

void main()
{	

    vec3 albedoSample = texture(albedo, fTexCoord).rgb;
    //albedoSample = pow(albedoSample, vec3(2.2));
    
    vec3 albedoCopy = albedoSample;
    
    vec3 specular = texture(specularTex, fTexCoord).rgb;
    float ao = specular.r;
    float roughness = specular.g;
    float metallic = specular.b;
    
    vec3 F0 = mix(vec3(0.04), albedoSample, metallic);
    
    vec3 maplessNormal = normalize(noNormalNormal);
    
    vec3 normal = texture(normalMap, fTexCoord).rgb * vec3(1, -1, 1) + vec3(0, 1, 0);
    normal = normalize((normal * 2.0 - 1.0) * TBN);
    normal = mix(maplessNormal, normal, normalStrength);

    vec3 lightDir = normalize(lightPos);
    vec3 viewDir = normalize(viewPos - FragPos); 
   

    float mipmapLevel = float(textureQueryLevels(skyBox));

    float shadow = ShadowCalculation(FragPosLightSpace, maplessNormal, lightDir);
   
    //albedoSample *= textureLod(skyBox, CubemapParallaxUV(normal).xyz, mipmapLevel * 0.8).rgb;
  
    vec4 cubemapUV = CubemapParallaxUV(reflect(-viewDir, normal));
    vec4 skyboxWithAlpha = textureLod(skyBox, cubemapUV.rgb , roughness * mipmapLevel);
    vec3 skyboxSampler = skyboxWithAlpha.rgb; 
    skyboxSampler = mix(skyboxSampler, textureLod(skyboxGlobal, reflect(-viewDir, normal), roughness * mipmapLevel).rgb, max(1-skyboxWithAlpha.a, cubemapUV.a));
    vec3 kS = fresnelSchlick(max(dot(normal, viewDir), 0.0), F0);
    vec3 kD = 1.0 - kS;
    kD *= 1.0 - metallic;  
  
    vec2 envBRDF  = texture(brdfLUT, vec2(max(dot(normal, viewDir), 0.0), roughness)).rg;
    
    skyboxSampler = skyboxSampler * (fresnelSchlickRoughness(max(dot(normal, viewDir), 0.0), F0, roughness) * envBRDF.x + envBRDF.y);
    

     vec4 irradianceSample = texture(irradianceTex, CubemapParallaxUV(normal).rgb);
     irradianceSample = vec4(mix(irradianceSample.rgb, texture(irradianceTex, reflect(-viewDir, normal)).rgb, cubemapUV.a),1);
   
      
     albedoSample *= irradianceSample.rgb;
     
     albedoSample = ((1-metallic) * albedoSample + skyboxSampler) * ao;

    albedoSample += calculateLight(lightDir, viewDir, roughness, F0, metallic, normal, shadow, albedoCopy, 0.5);
   
    for (int i = 0; i < lightsCount; i++) {
    
        vec3 pointLightPos = normalize(lights[i].Position - FragPos);
        
        float distance = length(lights[i].Position - FragPos);
        float num = clamp(1-pow(distance/(lights[i].intensity*10), 4), 0, 1);
        float attenuation = (num*num)/((distance*distance)+1+lights[i].radius);
   
        vec3 radiance = lights[i].Color * lights[i].intensity * attenuation;

        float pointShadow = 1 - ShadowCalculation(lights[i], shadowMaps[i], maplessNormal, viewPos);
        albedoSample += clamp(calculateLight(pointLightPos, viewDir, roughness, F0, metallic, normal, pointShadow, albedoCopy, lights[i].radius) * radiance,0,lights[i].intensity);
       
    }
 
    FragColor = vec4(albedoSample, 1.0);
    FragLoc = vec4(viewFragPos, metallic);
    FragNormal = vec4(viewNormal, dot(normalize(maplessNormal), viewDir));
}