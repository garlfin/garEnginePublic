#version 330

layout(location = 0) in vec3 vPosition;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

void main() {
    gl_Position = projection * model * view * vec4(vPosition, 1.0);
}

    #FRAGMENT
    #version 330

out vec4 FragColor;

void main() {
    FragColor = vec4(1);
}