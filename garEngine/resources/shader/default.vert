#version 330

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec3 vNormal;
layout(location = 2) in vec2 vTexCoord;

uniform mat4 mvp;
uniform mat4 model;

out vec2 fTexCoord;
out vec3 fNormal;
out vec3 FragPos;



void main() {
    fNormal = normalize(mat3(inverse(model)) * vNormal);
    fTexCoord = vTexCoord;
    gl_Position = mvp * vec4(vPosition, 1.0);
    FragPos = vec3(model * vec4(vPosition, 1.0));
}

