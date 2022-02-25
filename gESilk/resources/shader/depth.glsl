#version 330

layout(location = 0) in vec3 vPosition;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

void main() {
    gl_Position = vec4(vPosition, 1.0) * model * view * projection;
}
 -FRAGMENT-
    #version 330


void main() {
}