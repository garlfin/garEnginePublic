#version 330

uniform sampler2D albedo;
uniform samplerCube cubemap;
uniform sampler2D depthMap;
uniform sampler2D shadowMap;

in vec3 FragPos;
in vec2 fTexCoord;
in mat3 TBN;
in vec3 tangent;
in vec3 bitangent;
in vec3 fNormal;
in vec4 FragPosLightSpace;

in vec3 fViewVec;
in vec3 fLightPos;
uniform sampler2D normalMap;

out vec4 colorOut;

float heightScale = 0.05;

vec4 mixMultiply(vec4 inone, vec4 intwo, float mixfac){
    return mix(inone, inone*intwo, mixfac);
}

void main() {
    vec4 color = vec4(1.0); 
    float specFactor = 1.0 - 0.75;
    vec3 viewPos = normalize(fViewVec-FragPos);
    vec3 normal = texture(normalMap, fTexCoord).rgb;
    normal = normalize((normal * 2.0 - 1.0)*TBN);
    vec3 lightDir = normalize(fLightPos);  
   
    
    
    float ambient = max(dot(lightDir,normal),0.0)*0.5+0.5;

    float fresnel = clamp(1.0 - max(dot(viewPos, normal),0.0),0.0,1.0);
    // light reflected off normal, dot product with view vector
    float spec = clamp(pow(max(0,dot(reflect(lightDir, normal),-viewPos)),pow(12,1.0+specFactor)),0.0,1.0)*specFactor; 
    color = texture(albedo, fTexCoord)*ambient+vec4(spec);
    color = mixMultiply(color, texture(cubemap, reflect(-viewPos, normal)), fresnel * specFactor);
    color = pow(color, vec4(1.0/1.5));
    //colorOut = color;
    vec3 projCoords = FragPosLightSpace.xyz / FragPosLightSpace.w;
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float sampleColor = texture(shadowMap, projCoords.xy).r;
    colorOut = vec4(sampleColor); 
  
}