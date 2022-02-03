#version 330

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec3 vNormal;
layout(location = 2) in vec2 vTexCoord;
layout(location = 3) in vec3 vTangent;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;
uniform vec3 lightPos;

out vec3 FragPos;
out vec2 fTexCoord;
out mat3 TBN;
out vec3 fLightPos;

void main() {
    fTexCoord = vTexCoord;
    mat3 normalMatrix = transpose(inverse(mat3(model)));
    vec3 T = normalize(normalMatrix * vTangent);
    vec3 N = normalize(normalMatrix * vNormal);
    vec3 B = cross(N, T);
    TBN = transpose(mat3(T, B, N));
    FragPos = vec3(model * vec4(vPosition, 1.0));
    gl_Position = projection * model * view * vec4(vPosition, 1.0);
    fLightPos = normalize(lightPos);
}

#FRAGMENT
#version 330

uniform sampler2D albedo;
uniform sampler2D normalMap;

in vec3 FragPos;
in vec2 fTexCoord;
in mat3 TBN;
in vec3 fLightPos;

out vec4 FragColor;

void main() {
    //FragColor = texture(albedo, fTexCoord)*(max(0,dot(normal, lightPos))*0.5+0.5);

    vec3 normal = texture(normalMap, fTexCoord).rgb * vec3(1,-1,1) + vec3(0,1,0);
    normal = normalize((normal * 2.0 - 1.0)*TBN);

    float ambient = max(dot(fLightPos,normal),0.0)*0.5+0.5;
    
    FragColor = vec4(ambient); //vec4(max(0,dot(normal, normalize(lightPos)))*0.5+0.5);
}