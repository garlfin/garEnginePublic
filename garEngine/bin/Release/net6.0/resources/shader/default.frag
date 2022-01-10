#version 330

uniform sampler2D albedo;
uniform samplerCube cubemap;
uniform sampler2D depthMap;
uniform sampler2D shadowMap;

in vec3 FragPos;
in vec2 fTexCoord;
in mat3 TBN;
in mat4 viewMat;
in vec3 tangent;
in vec3 bitangent;
in vec3 fNormal;
in vec4 FragPosLightSpace;

in vec3 fViewVec;
in vec3 fLightPos;
uniform sampler2D normalMap;

layout(location = 0) out vec4 FragColor;
layout(location = 1) out vec4 NormalColor;
layout(location = 2) out vec4 FragPosition;

float heightScale = 0.05;

vec4 mixMultiply(vec4 inone, vec4 intwo, float mixfac){
    return mix(inone, inone*intwo, mixfac);
}

float shadowCalculation(vec4 fragPosLightSpace)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(shadowMap, projCoords.xy).r; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    // calculate bias (based on depth map resolution and slope)

    float bias = 0.005;
    // check whether current frag pos is in shadow
    // float shadow = currentDepth - bias > closestDepth  ? 1.0 : 0.0;
    // PCF
    float shadow = 0.0;
    vec2 texelSize = 2.0 / textureSize(shadowMap, 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
            shadow += currentDepth - bias > pcfDepth  ? 1.0 : 0.0;        
        }    
    }
    shadow /= 9.0;
    
    // keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
    if(projCoords.z > 1.0)
        shadow = 0.0;
        
    return 1.0 - shadow;
}

void main() {
   
    float specFactor = 1.0 - 0.75;
    vec3 viewPos = normalize(fViewVec-FragPos);
    vec3 normal = texture(normalMap, fTexCoord).rgb * vec3(1,-1,1) + vec3(0,1,0);
    normal = normalize((normal * 2.0 - 1.0)*TBN);
    vec3 lightDir = normalize(fLightPos);  
   
    float shadow = shadowCalculation(FragPosLightSpace);
    
    float ambient = max(dot(lightDir,normal),0.0)*0.5+0.5;

    float fresnel = clamp(1.0 - max(dot(viewPos, normal),0.0),0.0,1.0);
    // light reflected off normal, dot product with view vector
    float spec = clamp(pow(max(0,dot(reflect(lightDir, normal),-viewPos)),pow(12,1.0+specFactor)),0.0,1.0)*specFactor; 
    vec4 color = min(textureLod(cubemap, normal, 10)*2,1) * (texture(albedo, fTexCoord) * ambient + vec4(spec));
   
    color = mixMultiply(color, texture(cubemap, reflect(-viewPos, normal)), fresnel * specFactor);
    color = pow(color, vec4(1.0/2.2));
    
       
    FragColor = color;
    NormalColor = vec4(vec3(viewMat*vec4(normal,1.0)),1.0);
    FragPosition = vec4(vec3(viewMat*vec4(FragPos,1.0)),1.0);
    
  
}