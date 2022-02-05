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

out vec4 FragColor;


float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 lightDir)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    float currentDepth = projCoords.z;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)

    float bias = max(0.05 * (1.0 - dot(normal, lightDir)), 0.005);
    float shadow = texture(shadowMap, vec3(projCoords.xy, currentDepth-bias), 0);
    // get depth of current fragment from light's perspective
    // calculate bias (based on depth map resolution and slope)

    return clamp(shadow+0.75,0,1);
}

void main() {
    //FragColor = texture(albedo, fTexCoord)*(max(0,dot(normal, lightPos))*0.5+0.5);

    vec3 normal = texture(normalMap, fTexCoord).rgb * vec3(1,-1,1) + vec3(0,1,0);
    normal = normalize((normal * 2.0 - 1.0)*TBN);
    vec3 noNormalNormal = normalize(vec3(0,0,1)*TBN);

    vec3 lightDir = normalize(lightPos);
    float ambient = max(dot(lightDir,normal),0.0)*0.5+0.5;

    vec3 viewDir = normalize(FragPos-viewPos);
    
    float specFac = 1-0.2;
    float spec = clamp(pow(max(0.0, dot(reflect(lightDir, normal), viewDir)), pow(1+specFac, 8)),0,1)*specFac;
    
    vec4 color = texture(albedo, fTexCoord)*ambient+spec; //vec4(max(0,dot(normal, normalize(lightPos)))*0.5+0.5);
    //color = texture(skyBox, reflect(viewDir, normal));
    FragColor = pow(color, vec4(1/2.2)) * ShadowCalculation(FragPosLightSpace, noNormalNormal, lightDir);
}