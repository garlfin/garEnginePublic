#version 330

layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec3 vNormal;
layout(location = 2) in vec2 vTexCoord;
layout(location = 3) in vec3 vTangent;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

void main() {
    fTexCoord = vTexCoord;
    FragPos = vec3(model * vec4(vPosition, 1.0));  
    gl_Position = projection * view * model * vec4(vPosition, 1.0);
}

#FRAGMENT
#version 330

uniform sampler2D albedo;

in vec3 FragPos;
in vec2 fTexCoord;

out vec4 FragColor;

void main() {
    FragColor = texture(albedo, fTexCoord);
}