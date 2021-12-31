#version 330

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec3 vNormal;
layout(location = 2) in vec2 vTexCoord;
layout(location = 3) in vec3 vTangent;

uniform mat4 mvp;
uniform mat4 model;

uniform vec3 viewVec;
uniform vec3 lightPos;

out vec2 fTexCoord;
out vec3 FragPos;
out mat3 TBN;
out vec3 tangent;
out vec3 fViewVec;
out vec3 fLightPos;




void main() {
    fTexCoord = vTexCoord;
    mat3 normalMatrix = transpose(inverse(mat3(model)));
    vec3 T = normalize(normalMatrix * vTangent);
    vec3 N = normalize(normalMatrix * vNormal);
    vec3 B = cross(N, T);
    
    tangent = T;
    TBN = transpose(mat3(T, B, N));   
    fViewVec = TBN * viewVec;
    fLightPos = TBN * lightPos;
    FragPos = vec3(model * vec4(vPosition, 1.0));  
    gl_Position = mvp * vec4(vPosition, 1.0);
}

