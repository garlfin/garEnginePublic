#version 330

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
    vec3 T = normalize(vTangent * normalMatrix );
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

#FRAGMENT
#version 330

uniform samplerCube skyBox;
uniform sampler2D albedo;
uniform sampler2D normalMap;
uniform sampler2DShadow shadowMap;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform float emission = 1.0;
uniform float roughness = 0.8;
uniform float metallic = 0.0;
uniform float ior = 1.450;

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
const float totalTexels = (pcfCount * 2.0+1.0)*(pcfCount*2.0+1.0);

float fresnelSchlick(float cosi, float eta)
{
    /* compute fresnel reflectance without explicitly computing
     * the refracted direction */
    float c = abs(cosi);
    float g = eta * eta - 1.0 + c * c;
    float result;

    if (g > 0.0) {
        g = sqrt(g);
        float A = (g - c) / (g + c);
        float B = (c * (g + c) - 1.0) / (c * (g - c) + 1.0);
        result = 0.5 * A * A * (1.0 + B * B);
    }
    else {
        result = 1.0; /* TIR (no refracted component) */
    }

    return result;
}

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    float currentDepth = projCoords.z;
    float bias = max(0.001 * (1.0 - dot(normal, lightDir)), 0.001);
    float visibility = 0;

    float texelSize = 1.0/textureSize(shadowMap,0).x;
    for (int x = -pcfCount; x<= pcfCount; x++){
        for (int y =-pcfCount; y<=pcfCount; y++){
            visibility += texture( shadowMap, vec3(projCoords.xy+vec2(x,y)*texelSize,  currentDepth - bias));
        }
    }
    visibility /= totalTexels;
    if(projCoords.z > 1.0) visibility = 1;
    
    return visibility;
}



void main() {

    vec3 normal = texture(normalMap, fTexCoord).rgb * vec3(1,-1,1) + vec3(0,1,0);
    normal = normalize((normal * 2.0 - 1.0)*TBN);
    vec3 viewDir = normalize(FragPos-viewPos);

    vec3 lightDir = normalize(lightPos);
    vec4 ambient = textureLod(skyBox, normal, 10)*0.2;
    float ambientLambert = max(dot(lightDir,normal),0.0)*0.5+0.5;
    float specFac = 1-roughness;
    float spec = clamp(pow(max(0.0, dot(reflect(lightDir, normal), viewDir)), pow(1+specFac, 8)),0,1)*specFac;
    ambient = ambient + ambientLambert * clamp(ShadowCalculation(FragPosLightSpace, noNormalNormal, lightDir)+0.5,0,1);

    
    vec4 color = texture(albedo, fTexCoord);
    float fresnelFac = (specFac*max(fresnelSchlick(dot(normal, viewDir),ior),0));
    vec4 specWithSkybox = textureLod(skyBox, reflect(viewDir, normal), int((1-specFac)*10))+spec;
    color = mix( color + (specWithSkybox*fresnelFac), color * specWithSkybox, metallic);
    
    FragColor = vec4(vec3(color*mix(ambient, vec4(1), metallic)*emission),1.0); //vec4(vec3(max(fresnelSchlick(dot(normal, viewDir),ior),0)),1.0);
    FragLoc = vec4(viewFragPos,1.0);
    FragNormal = vec4(viewNormal, dot(noNormalNormal, viewDir));
}