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


void main() {
    fTexCoord = vTexCoord;
    mat3 normalMatrix =  mat3(transpose(inverse(model)));
    vec3 T = normalize(vTangent * normalMatrix );
    vec3 N = normalize(vNormal * normalMatrix);
    vec3 B = cross(N, T);
    TBN = transpose(mat3(T, B, N));
    FragPos = vec3(vec4(vPosition, 1.0) * model);
    vec4 worldPos = vec4(vPosition, 1.0) * model;
    gl_Position = worldPos * view * projection;
    FragPosLightSpace = worldPos * lightView * lightProjection;
    
}

#FRAGMENT
#version 330

uniform samplerCube skyBox;
uniform sampler2D albedo;
uniform sampler2D normalMap;
uniform sampler2DShadow shadowMap;
uniform vec3 lightPos;
uniform vec3 viewPos;

in vec3 FragPos;
in vec2 fTexCoord;
in mat3 TBN;
in vec4 FragPosLightSpace;

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec3 FragNormal;
layout (location = 2) out vec3 FragLoc;

float random(vec3 seed, int i){
    vec4 seed4 = vec4(seed,i);
    float dot_product = dot(seed4, vec4(12.9898,78.233,45.164,94.673));
    return fract(sin(dot_product) * 43758.5453);
}

const int pcfCount = 2;
const float totalTexels = (pcfCount * 2.0+1.0)*(pcfCount*2.0+1.0);

float fresnelSchlick(float cosTheta)
{
    return pow(1.0 - cosTheta, 5.0);
}

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    float currentDepth = projCoords.z;
    float bias = max(0.01 * (1.0 - dot(normal, lightDir)), 0.001);
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
    vec3 noNormalNormal = normalize(vec3(0,0,1)*TBN);

    vec3 lightDir = normalize(lightPos);
    vec4 ambient = FragColor = textureLod(skyBox, normal, 10)*0.25 + (max(dot(lightDir,normal),0.0)*0.5+0.5) * ShadowCalculation(FragPosLightSpace, noNormalNormal, lightDir);

    vec3 viewDir = normalize(FragPos-viewPos);
    
    float specFac = 1-0.8;
    float spec = clamp(pow(max(0.0, dot(reflect(lightDir, normal), viewDir)), pow(1+specFac, 8)),0,1)*specFac;
    vec4 color = texture(albedo, fTexCoord)+spec;
    color = mix(color, textureLod(skyBox, reflect(viewDir, normal), int((1-specFac)*10)),specFac*clamp(fresnelSchlick(dot(normal, normalize(viewPos))),0,1));
    
    FragColor = color*ambient;
    FragLoc = FragPos;
    FragNormal = normal;
}